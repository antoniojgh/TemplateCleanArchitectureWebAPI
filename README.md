# DientesLimpios — Clean Architecture Web API (.NET 10)

A dental clinic backend (Patients, Dentists, Offices, Appointments) built as a
**reference implementation** of Clean Architecture, DDD and CQRS on .NET 10.

It is a portfolio project, so it optimises for correctness and for explaining
**why** each decision was made, not for feature count. Every section below points
at the code that implements it.

- **Stack:** .NET 10, EF Core 10, SQL Server, ASP.NET Core Identity, Serilog
- **Patterns:** CQRS with a custom mediator, Result pattern, rich domain model,
  domain events, Transactional Outbox, repositories and interceptors
- **Tests:** unit (xUnit + NSubstitute), architecture (NetArchTest), end-to-end
  integration against a real SQL Server (Testcontainers)

---

## Table of contents

- [Architecture](#architecture)
- [Design decisions and the reasoning behind them](#design-decisions-and-the-reasoning-behind-them)
- [Domain model](#domain-model)
- [API surface](#api-surface)
- [Getting started](#getting-started)
- [Testing](#testing)
- [Project layout](#project-layout)
- [Known limitations](#known-limitations)
- [License](#license)

---

## Architecture

Dependencies point inwards. The Domain knows nothing about anything else — not
even EF Core.

```text
DientesLimpios.API              composition root: controllers, DI, jobs
   │  references everything
   ├── DientesLimpios.Persistence      EF Core, outbox, repositories, interceptors
   ├── DientesLimpios.Infrastructure   SMTP notifications
   ├── DientesLimpios.Identity         ASP.NET Core Identity, bearer tokens
   │        │
   │        └── all three reference ▼
   └── DientesLimpios.Application      use cases, interfaces, validators, mediator
                │
                └── DientesLimpios.Domain    aggregates, value objects, events
                                             (no dependencies at all)
```

Outer layers implement interfaces declared in `Application/Interfaces/`, so the
core never takes a dependency on a database or an SMTP client.

**These rules are enforced, not merely documented.** `DientesLimpios.ArchitectureTests`
uses NetArchTest to fail the build if Domain ever references
`Microsoft.EntityFrameworkCore`, `Microsoft.AspNetCore`, `FluentValidation` or
`MediatR`, or if Application references an outward project.

---

## Design decisions and the reasoning behind them

### A custom mediator instead of MediatR

`SimpleMediator` (`Application/Utilities/Mediator/`) resolves
`IRequestHandler<TRequest, TResponse>` from the container, runs the matching
FluentValidation validator first, and short-circuits to
`Result.Failure(ValidationError)` when validation fails.

**Why:** MediatR moved to a commercial licence. In this codebase the library was
a thin layer of indirection, so replacing it was cheaper than taking on a paid
dependency — and it demonstrates the pattern itself rather than the package.

**Trade-off:** there is no pipeline-behaviour support yet, so per-handler logging
is repeated. Handlers are still discovered automatically (Scrutor assembly
scanning), so adding a use case requires no DI registration.

### Hand-written mapping instead of AutoMapper

Each use-case folder owns a `MapperExtensions` with explicit
`Appointment → AppointmentDetailDTO` methods.

**Why:** mapping mistakes become compile errors instead of runtime surprises, the
projection is visible where it is used, and queries can project straight into a
DTO without loading whole entities.

### Result pattern instead of exceptions for business outcomes

Domain and Application return `Result` / `Result<T>` carrying an `Error` from the
`DomainErrors` catalogue. Exceptions are reserved for genuine faults.

```csharp
public static Result<Appointment> Create(Guid patientId, Guid dentistId, Guid officeId,
                                         DateTime startDate, DateTime endDate, DateTime nowUtc)
{
    if (startDate < nowUtc)
        return Result.Failure<Appointment>(DomainErrors.Appointment.InThePast);
    ...
}
```

**Why:** "this slot is taken" is an expected outcome, not an exceptional one.
Making it a return value keeps it in the method signature, where callers cannot
overlook it.

**How it reaches HTTP:** `API/Extensions/ResultExtensions.cs` maps an error code
to a status code by suffix convention — `*.NotFound` to 404, `*.Conflict` and
`*.Overlapping` to 409, everything else to 400 — and renders an RFC 9457
`ProblemDetails` carrying the code in an `errorCode` extension:

```json
{
  "type": "https://httpstatuses.io/409",
  "title": "Conflict",
  "status": 409,
  "detail": "The dentist already has an appointment at that time.",
  "instance": "/api/v1/appointments",
  "errorCode": "Appointment.Overlapping"
}
```

Clients branch on the stable `errorCode` instead of parsing prose.

### A rich domain model

Aggregates inherit `AggregateRoot`, are built only through static factories
returning `Result<T>`, keep private setters, and expose behaviour — `Cancel()`,
`Complete()`, `MarkConfirmationSent()` — rather than data. Value objects (`Email`,
`TimeInterval`) are `sealed record`s with their own `Create` validation, mapped
with EF Core `ComplexProperty`. Ids are `Guid.CreateVersion7()`, which keeps
primary keys sequential and index-friendly.

**Why:** an `Appointment` that exists is always valid. No path produces one whose
end precedes its start, so handlers never re-check invariants.

### Transactional Outbox for reliable side effects

Booking an appointment must confirm it by email. Sending that email inside the
HTTP request couples booking to SMTP; dispatching the event in memory after
commit loses it when delivery fails.

The flow (`Persistence/Outbox/`):

1. `Appointment.Create` raises `AppointmentCreatedEvent` inside the aggregate.
2. `InsertOutboxMessagesInterceptor`, a `SaveChangesInterceptor`, serialises every
   pending event into an `OutboxMessages` row **during the same `SaveChanges`**,
   so event and appointment commit — or roll back — together.
3. `OutboxProcessorJob` polls every 5 seconds. `OutboxProcessor` claims a batch
   with `WITH (UPDLOCK, READPAST, ROWLOCK)`, so several API instances can run at
   once without ever picking up the same message.
4. Each message is dispatched in its own DI scope and then marked
   `ProcessedOnUtc`, or `AttemptCount` plus `Error` when the handler throws.
   Retries stop after 5 attempts.

**Guarantee: at-least-once.** `AppointmentCreatedEmailHandler` is therefore
idempotent — it checks `Appointment.ConfirmationSentAtUtc` before sending and
records it afterwards. That narrows, but cannot close, the window between sending
an email and recording the send, because SMTP has no idempotency key.

The request never waits for SMTP, and a failed email is retried rather than lost
in a log line.

### Concurrency: never double-book a dentist

Checking "does this slot overlap?" and then inserting is a
time-of-check/time-of-use race — two simultaneous requests both see a free slot.
"These two intervals do not overlap" cannot be expressed as a unique index
either.

`AppointmentRepository.AddIfNoOverlap` opens a transaction and takes an
application lock keyed by dentist (`sp_getapplock`) before the check, so bookings
for *different* dentists never block each other. An integration test fires two
overlapping requests concurrently and asserts exactly one 201 and one 409.

### Appointments survive deletes

All three foreign keys use `DeleteBehavior.Restrict`. Deleting a dentist who has
history returns 409 with an `errorCode`, and the database refuses the delete as a
backstop. Appointments are medical and financial history.

### Cross-cutting persistence lives in interceptors

`AuditableEntitiesInterceptor` stamps created and modified fields;
`InsertOutboxMessagesInterceptor` writes outbox rows. Neither is a `DbContext`
override, so the `DbContext` stays a mapping concern and each behaviour can be
tested on its own.

### A strict build

`Directory.Build.props` enables `TreatWarningsAsErrors`, `AnalysisMode=All`,
`EnforceCodeStyleInBuild` and nullable reference types. Analyzer suppressions live
in `.editorconfig`, each with a comment explaining why. Package versions are
centralised in `Directory.Packages.props` (Central Package Management), including
transitive pins that close known CVEs.

**Why:** in a reference project the build is the reviewer. A warning allowed to
survive is a convention that is not really enforced.

### Tests that exercise the real thing

Integration tests run against **SQL Server in Docker** through Testcontainers and
apply the real migrations, so `sp_getapplock`, `READPAST` and filtered indexes
behave as they do in production — unlike an in-memory provider, which would
happily accept code SQL Server rejects.

---

## Domain model

| Aggregate | Invariants enforced in the domain |
|---|---|
| `Appointment` | starts before it ends; never in the past; only a scheduled appointment can be cancelled or completed; a confirmation is recorded once |
| `Patient` | name required; `Email` value object validates the format |
| `Dentist` | name required; `Email` value object validates the format |
| `Office` | name required |

Supporting types: `TimeInterval` (validated start and end), `Email`,
`AppointmentStatus` (`Scheduled`, `Completed`, `Cancelled`) and
`AppointmentCreatedEvent`.

---

## API surface

Endpoints are versioned by URL segment (`/api/v1/...`) and require authentication
by default through a global `AuthorizeFilter`. Identity endpoints (register,
login, refresh) are mapped by `MapIdentityApi<User>` and issue bearer tokens.

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/v1/appointments` | list, filtered by office, dentist, patient, status or date range |
| `GET` | `/api/v1/appointments/{id}` | detail |
| `POST` | `/api/v1/appointments` | book: overlap-checked, raises the confirmation event |
| `POST` | `/api/v1/appointments/complete/{id}` | mark completed |
| `POST` | `/api/v1/appointments/cancel/{id}` | cancel |
| `POST` | `/api/v1/appointments/reminder` | trigger the reminder run manually |
| `GET/POST/PUT/DELETE` | `/api/v1/patients[/{id}]` | CRUD, paginated list |
| `GET/POST/PUT/DELETE` | `/api/v1/dentists[/{id}]` | CRUD, paginated list |
| `GET/POST/PUT/DELETE` | `/api/v1/offices[/{id}]` | CRUD |

Paginated lists return the total row count in a `total-number-of-records`
response header. Failures are always `ProblemDetails` with an `errorCode`.

A background job sends next-day reminders daily at 08:00 Europe/Madrid.

OpenAPI is exposed in Development at `/openapi/v1.json`. The `.http` files in
`DientesLimpios.API/` hold ready-to-run requests for every controller.

---

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server: LocalDB, a container, or a real instance
- Docker, only to run the integration tests

### 1. Clone and build

```bash
git clone https://github.com/antoniojgh/TemplateCleanArchitectureWebAPI.git
cd TemplateCleanArchitectureWebAPI
dotnet build
```

### 2. Configure

The connection string key is `ConnectionStrings:DientesLimpiosConnectionString`.
`appsettings.Development.json` points at LocalDB out of the box.

Email settings belong in user secrets, never in a committed file. The API project
already has a `UserSecretsId`:

```bash
dotnet user-secrets set "Email:Host" "smtp.example.com" --project DientesLimpios.API
dotnet user-secrets set "Email:Port" "587" --project DientesLimpios.API
dotnet user-secrets set "Email:Email" "no-reply@example.com" --project DientesLimpios.API
dotnet user-secrets set "Email:Password" "<app password>" --project DientesLimpios.API
```

Email options are validated at startup with
`ValidateDataAnnotations().ValidateOnStart()`, so a missing value fails fast
instead of at the first send.

### 3. Create the database

There are two `DbContext`s, each with its own migrations folder:

```bash
# Domain schema: patients, dentists, offices, appointments, outbox
dotnet ef database update --project DientesLimpios.Persistence --startup-project DientesLimpios.API --context DientesLimpiosDbContext

# Identity schema: users, roles, tokens
dotnet ef database update --project DientesLimpios.Identity --startup-project DientesLimpios.API --context DientesLimpiosIdentityDbContext
```

### 4. Run

```bash
dotnet run --project DientesLimpios.API
```

The API listens on `https://localhost:7199` and `http://localhost:5298`. Register
a user through the Identity endpoints, then call the API with the bearer token.
Endpoints require the `esadmin` claim.

---

## Testing

```bash
dotnet test                                   # everything
dotnet test DientesLimpios.Tests              # unit tests only: fast, no Docker
dotnet test DientesLimpios.ArchitectureTests
dotnet test DientesLimpios.IntegrationTests   # needs Docker running
```

| Project | Covers |
|---|---|
| `DientesLimpios.Tests` | domain invariants, use-case handlers with NSubstitute mocks, `ProblemDetails` mapping, outbox serialisation |
| `DientesLimpios.ArchitectureTests` | the dependency rules above, via NetArchTest |
| `DientesLimpios.IntegrationTests` | full HTTP-to-SQL-Server round trips: booking, 409 on overlap, concurrent double booking, restricted deletes, outbox persistence, retry after a failed send, and no duplicate email on redelivery |

Integration tests start a `mcr.microsoft.com/mssql/server:2022-latest` container,
apply the migrations, replace SMTP with a recording fake, and swap bearer auth for
a test scheme. Without Docker they fail at `InitializeAsync`; run the other two
projects instead.

---

## Project layout

```text
DientesLimpios.slnx
├── DientesLimpios.Domain              Aggregates, value objects, domain events, Result, DomainErrors
├── DientesLimpios.Application         Use cases (Commands/Queries), interfaces, validators, SimpleMediator
├── DientesLimpios.Persistence         DbContext, configurations, migrations, repositories, interceptors, Outbox
├── DientesLimpios.Infrastructure      SMTP notification service
├── DientesLimpios.Identity            ASP.NET Core Identity, users, bearer tokens
├── DientesLimpios.API                 Controllers, DTOs, versioning, exception handling, background jobs
├── DientesLimpios.Tests               Unit tests
├── DientesLimpios.ArchitectureTests   NetArchTest dependency rules
└── DientesLimpios.IntegrationTests    Testcontainers end-to-end tests
```

Every use case is a folder —
`Application/UseCases/<Aggregate>/{Commands,Queries}/<Name>/` — holding its
command or query, handler, validator and DTO together.

---

## Known limitations

Listed deliberately: a reference project should be honest about what it does not
do yet.

- **No CI.** `.github/workflows/` is empty; build and tests run locally.
- **Email delivery is at-least-once**, and a message that fails five times stays
  in `OutboxMessages` with its error. There is no retry backoff, dead-letter view
  or alerting.
- **Data access is inconsistent**: some handlers use `IApplicationDbContext`
  directly, others go through repositories, and a few use both. The intended
  direction is commands through repositories, queries projecting straight to DTOs.
- **Validation is duplicated** across API DataAnnotations, FluentValidation and
  the domain factories, with rules that do not always agree.
- **No optimistic-concurrency tokens**, and the appointment-overlap query has no
  covering index.
- **`SimpleMediator` has no pipeline behaviours**, so logging is repeated in every
  handler.
- **Spanish remnants** from the original codebase are still being renamed
  (`ADto`, `AgregarServicesDeX`, the `esadmin` policy).

---

## License

MIT — see [LICENSE.txt](LICENSE.txt).
