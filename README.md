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

The dentist, office and patient queries map a loaded entity with an explicit
`MapperExtensions` in the use-case folder. The appointment queries skip that step
altogether: the handler projects from the `DbContext` straight into its DTO, so
the `select new` *is* the mapping.

**Why:** mapping mistakes become compile errors instead of runtime surprises, and
the projection is visible where it is used. Where a DTO needs data from more than
one aggregate — an appointment plus the patient, dentist and office names —
projecting in the query is also the cheaper path: three joined columns are read
instead of three whole aggregates being materialised to copy one string from each.

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
`Complete()`, `Reschedule()`, `MarkConfirmationSent()` — rather than data. Value objects (`Email`,
`TimeInterval`) are `sealed record`s with their own `Create` validation, mapped
with EF Core `ComplexProperty`. Ids are `Guid.CreateVersion7()`, which keeps
primary keys sequential and index-friendly.

One aggregate never holds a reference to another: an `Appointment` carries
`PatientId`, `DentistId` and `OfficeId` and no navigation properties. Reads join
on those ids, so nothing tempts a handler to reach across an aggregate boundary
and nothing depends on a caller having remembered an `Include`.

**Why:** an `Appointment` that exists is always valid. No path produces one whose
end precedes its start, so handlers never re-check invariants.

### Transactional Outbox for reliable side effects

Booking an appointment must confirm it by email. Sending that email inside the
HTTP request couples booking to SMTP; dispatching the event in memory after
commit loses it when delivery fails.

The flow (`Persistence/Outbox/`):

1. `Appointment.Create` raises `AppointmentCreatedEvent`, `Appointment.Cancel`
   raises `AppointmentCancelledEvent`, and `Appointment.Reschedule` raises
   `AppointmentRescheduledEvent`, inside the aggregate. Each email handler reads
   the current appointment with a join and sends the matching email.
2. `InsertOutboxMessagesInterceptor`, a `SaveChangesInterceptor`, serialises every
   pending event into an `OutboxMessages` row **during the same `SaveChanges`**,
   so event and appointment commit — or roll back — together.
3. `OutboxProcessorJob` polls on a fixed interval (its `PollingInterval`
   constant). `OutboxProcessor` claims a batch with
   `WITH (UPDLOCK, READPAST, ROWLOCK)`, so several API instances can run at once
   without ever picking up the same message.
4. Each message is dispatched in its own DI scope and then marked
   `ProcessedOnUtc`, or `AttemptCount` plus `Error` when the handler throws.
   Retries stop after 5 attempts.

**Guarantee: at-least-once.** Email handlers are therefore made idempotent through
a delivery marker on the appointment: `AppointmentCreatedEmailHandler` checks
`Appointment.ConfirmationSentAtUtc` before sending and records it afterwards, and
`AppointmentCancelledEmailHandler` does the same with
`Appointment.CancellationSentAtUtc`. That narrows, but cannot close, the window
between sending an email and recording the send, because SMTP has no idempotency
key. An appointment can be rescheduled many times, so
`AppointmentRescheduledEmailHandler` keys its marker by event instead:
`Appointment.RescheduleNoticeEventId` holds the `EventId` of the last reschedule
notified, so a redelivered message is skipped and the next reschedule still gets
its own email.

The request never waits for SMTP, and a failed email is retried rather than lost
in a log line.

### Concurrency: no double bookings, no silent overwrites

Checking "does this slot overlap?" and then inserting is a
time-of-check/time-of-use race — two simultaneous requests both see a free slot.
"These two intervals do not overlap" cannot be expressed as a unique index
either.

`AppointmentRepository.AddIfNoOverlap` opens a transaction and takes an
application lock keyed by dentist (`sp_getapplock`) before the check, so bookings
for *different* dentists never block each other. An integration test fires two
overlapping requests concurrently and asserts exactly one 201 and one 409.

Rescheduling takes the same lock in `RescheduleIfNoOverlap`, and saves before
committing, so a move and a new booking for the same dentist cannot both claim a
slot. The overlap check ignores the appointment being moved: pushing a 10:00–11:00
appointment back to 10:30–11:30 is allowed, and the slot it leaves is free again
immediately.

Editing is guarded separately. Every aggregate carries a `rowversion` token, so a
second writer starting from a stale copy is refused rather than overwriting the
first — two staff members cancelling and completing the same appointment no longer
end in "last write wins". The conflict comes back as a 409 with
`errorCode: "Concurrency.Conflict"`, not a 500.

### Appointments survive deletes

All three foreign keys use `DeleteBehavior.Restrict`, configured from the
appointment side without navigation properties
(`HasOne<Patient>().WithMany().HasForeignKey(a => a.PatientId)`). Deleting a dentist who has
history returns 409 with an `errorCode`, and the database refuses the delete as a
backstop. Appointments are medical and financial history.

### Time: UTC inside, clinic time at the edges

Scheduling systems collect time bugs, so the convention is explicit and enforced
at every boundary: **every `DateTime` inside the application is UTC**, and a local
clock time exists only where a person reads it.

- **API:** a `JsonConverter` accepts only ISO 8601 with an offset or `Z`, and
  always answers with `Z`. `10:00:00` with no zone is ambiguous, so it is a 400
  rather than a guess. Query-string filters are bound to UTC and validated.
- **Database:** an EF Core convention puts a value converter on every `DateTime`
  column. Writing a non-UTC value throws; every value read back is marked UTC, so
  it compares correctly in memory and serialises with `Z`.
- **Clock:** code never reads `DateTime.UtcNow`. It takes an injected
  `TimeProvider`, and the Domain receives "now" as a parameter, so tests can pin
  time, or advance it with `FakeTimeProvider` for the reminder job's schedule.
- **Clinic time:** "tomorrow" for reminders, and the times in emails, are computed
  in the clinic's zone (`Clinic:TimeZoneId`), then converted back to UTC for
  queries.

**Why UTC `DateTime` rather than `DateTimeOffset`:** for a physical clinic, the
offset that matters is the clinic's, not the caller's. Preserving each client's
offset would buy little, while UTC keeps comparisons and SQL predicates simple.

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
| `Appointment` | starts before it ends; never booked or rescheduled into the past; only a scheduled appointment can be cancelled, completed or rescheduled; a confirmation is recorded once |
| `Patient` | name required; `Email` value object validates the format |
| `Dentist` | name required; `Email` value object validates the format |
| `Office` | name required |

Supporting types: `TimeInterval` (validated start and end), `Email`,
`AppointmentStatus` (`Scheduled`, `Completed`, `Cancelled`),
`AppointmentCreatedEvent`, `AppointmentCancelledEvent` and
`AppointmentRescheduledEvent`.

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
| `POST` | `/api/v1/appointments/reschedule` | move to a new slot (`id`, `startDate`, `endDate` in the body): overlap-checked, raises the rescheduled event |
| `POST` | `/api/v1/appointments/reminder` | trigger the reminder run manually |
| `GET/POST/PUT/DELETE` | `/api/v1/patients[/{id}]` | CRUD, paginated list |
| `GET/POST/PUT/DELETE` | `/api/v1/dentists[/{id}]` | CRUD, paginated list |
| `GET/POST/PUT/DELETE` | `/api/v1/offices[/{id}]` | CRUD |

Paginated lists return the total row count in a `total-number-of-records`
response header. Failures are always `ProblemDetails` with an `errorCode`.

A background job sends next-day reminders daily at 08:00 clinic time
(`Clinic:TimeZoneId`, `Europe/Madrid` by default).

OpenAPI is exposed in Development at `/openapi/v1.json`. The `.http` files in
`DientesLimpios.API/` hold ready-to-run requests for every controller.

### Time contract

Every instant is ISO 8601. Requests must include an offset or `Z`; responses are
always UTC with `Z`. A time without a zone is rejected with 400 instead of being
guessed.

```http
POST /api/v1/appointments
Content-Type: application/json
Authorization: Bearer <token>

{
  "patientId": "0199a0d4-5c6e-7b1a-9f2e-3c4d5e6f7a81",
  "dentistId": "0199a0d4-5c6e-7b1a-9f2e-3c4d5e6f7a82",
  "officeId": "0199a0d4-5c6e-7b1a-9f2e-3c4d5e6f7a83",
  "startDate": "2030-09-01T10:00:00+02:00",
  "endDate": "2030-09-01T11:00:00+02:00"
}
```

`GET /api/v1/appointments/{id}` returns the same instants in UTC:

```json
{
  "id": "0199a0d4-5c6e-7b1a-9f2e-3c4d5e6f7a90",
  "patient": "Jane Doe",
  "dentist": "John Smith",
  "office": "Main Office",
  "startDate": "2030-09-01T08:00:00Z",
  "endDate": "2030-09-01T09:00:00Z",
  "appointmentStatus": "Scheduled"
}
```

The confirmation email shows the start as 10:00, the clinic's local time.

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

The clinic's time zone drives the reminder schedule and the times shown in emails.
It is `Europe/Madrid` in `appsettings.json` and is validated at startup, so an
unknown id stops the app immediately. Override it like any other setting, for
example with the environment variable `Clinic__TimeZoneId=Atlantic/Canary`.

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
| `DientesLimpios.Tests` | domain invariants, use-case handlers with NSubstitute mocks, validators and the reminder window with a pinned clock, the reminder job schedule with `FakeTimeProvider`, the JSON time converter, `ProblemDetails` mapping, outbox serialisation |
| `DientesLimpios.ArchitectureTests` | the dependency rules above, via NetArchTest |
| `DientesLimpios.IntegrationTests` | full HTTP-to-SQL-Server round trips: booking, 409 on overlap, concurrent double booking, restricted deletes, outbox persistence, retry after a failed send, no duplicate email on redelivery, and the time contract (offset in, `Z` out, zone-less input rejected) |

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
- **Data access is still mixed.** The appointment use cases now follow the
  intended split — commands load the aggregate through a repository, queries
  project straight to DTOs — but `CreateAppointmentHandler` uses both styles, and
  the dentist, office and patient queries still load entities and map them.
- **Validation is duplicated** across API DataAnnotations, FluentValidation and
  the domain factories, with rules that do not always agree.
- **The concurrency token is not exposed to clients.** Aggregates carry a
  `rowversion`, but a client that reads, waits and writes back still wins, because
  the handler reloads inside the request. Closing that needs an `ETag` with
  `If-Match`.
- **The appointment-overlap query has no covering index.**
- **`SimpleMediator` has no pipeline behaviours**, so logging is repeated in every
  handler.
- **One time zone for all offices** (`Clinic:TimeZoneId`). Offices in different
  zones would need a per-office time zone.
- **The reminder schedule is approximate.** The job checks the clock hourly, so it
  fires somewhere between 08:00 and 08:59. A restart during that hour sends the
  reminders twice, and downtime covering the whole hour skips the day.
- **Spanish remnants** from the original codebase are still being renamed
  (`ADto`, `AgregarServicesDeX`, the `esadmin` policy).

---

## License

MIT — see [LICENSE.txt](LICENSE.txt).
