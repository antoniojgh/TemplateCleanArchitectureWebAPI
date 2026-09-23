# Follow this

This project is reviewed and implemented by me. When asked about improvements, explain the reasoning and show the proposed code; 
do not implement unless I explicitly say "implement it".

# DientesLimpios — .NET 10 Clean Architecture Web API

Dental clinic backend (Patients, Dentists, Offices, Appointments). Portfolio
reference project: correctness and clarity matter more than speed of delivery.

**Answer and write code in English.** The codebase was migrated from Spanish;
do not introduce new Spanish identifiers, comments, or messages.

---

## Stack

| Concern | Choice |
|---|---|
| Framework | .NET 10 (`net10.0`), C# `latest` |
| Architecture | Clean Architecture + DDD + CQRS |
| ORM | EF Core 10, SQL Server |
| Mediator | **Custom `SimpleMediator`** — NOT MediatR |
| Mapping | **Hand-written extension methods** — NOT AutoMapper |
| Validation | FluentValidation (commands) + domain factories |
| Auth | ASP.NET Core Identity + `MapIdentityApi`, bearer tokens |
| Logging | Serilog (structured) |
| Reliability | Transactional Outbox (`OutboxMessages` + `OutboxProcessorJob`) |
| Tests | xUnit, NSubstitute, FluentAssertions, NetArchTest, Testcontainers, FakeTimeProvider (only for code that waits on time) |
| API | Controllers + URL-segment versioning (`/api/v1/...`), OpenAPI |

---

## Commands

```bash
dotnet build                      # solution: DientesLimpios.slnx
dotnet test                       # all three test projects
dotnet test DientesLimpios.Tests  # unit tests only (fast, no Docker)
```

`DientesLimpios.IntegrationTests` starts a SQL Server container via
Testcontainers — **Docker must be running** or those tests fail at
`InitializeAsync`. When Docker is unavailable, run the unit and architecture
projects individually rather than reporting a broken build.

Migrations (two separate DbContexts, two separate migration folders):

```bash
dotnet ef migrations add <Name> --project DientesLimpios.Persistence --startup-project DientesLimpios.API --context DientesLimpiosDbContext
dotnet ef migrations add <Name> --project DientesLimpios.Identity   --startup-project DientesLimpios.API --context DientesLimpiosIdentityDbContext
dotnet ef database update --project DientesLimpios.Persistence --startup-project DientesLimpios.API --context DientesLimpiosDbContext
```

Connection string key: `ConnectionStrings:DientesLimpiosConnectionString`
(local dev in `appsettings.Development.json`; secrets belong in user-secrets —
`UserSecretsId` is already set on the API project).

---

## Build is strict — this will bite you

`Directory.Build.props` sets:

```xml
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
<AnalysisMode>All</AnalysisMode>
<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
<Nullable>enable</Nullable>
```

**Any analyzer warning fails the build.** Before claiming a change works, run
`dotnet build` — do not assume. If a new CA rule fires, fix the code first;
only suppress in `.editorconfig` with a comment explaining why, matching the
style of the existing suppressions.

**Central Package Management is on.** `Directory.Packages.props` owns every
version. Add a package by putting `<PackageVersion Include="X" Version="Y" />`
there and a bare `<PackageReference Include="X" />` in the csproj. Never put a
`Version` attribute in a csproj. Some pins exist solely to close transitive
CVEs — keep those comments.

---

## Project layout and dependency rules

```
DientesLimpios.Domain          ← no dependencies at all (not even EF Core)
DientesLimpios.Application     ← Domain only
DientesLimpios.Persistence     ← Application + Domain
DientesLimpios.Infrastructure  ← Application
DientesLimpios.Identity        ← Application
DientesLimpios.API             ← everything (composition root)
```

`DientesLimpios.ArchitectureTests` enforces this with NetArchTest. If a change
requires a new project reference, it is almost certainly the wrong change —
add an interface in `Application/Interfaces/` and implement it outward.

Domain must never reference `Microsoft.EntityFrameworkCore`,
`Microsoft.AspNetCore`, `FluentValidation`, or `MediatR`. There is an
architecture test for exactly this.

---

## Conventions to follow

### Result pattern, not exceptions

Domain and Application signal failure with `Result` / `Result<T>` and an
`Error` from the static `DomainErrors` catalogue. Do not throw for expected
business outcomes.

```csharp
public static Result<Dentist> Create(string name, string email)
{
    if (string.IsNullOrWhiteSpace(name))
        return Result.Failure<Dentist>(DomainErrors.Dentist.NameRequired);
    ...
}
```

New errors go in `Domain/Errors/DomainErrors.cs` under the right nested class.
Error codes drive HTTP status mapping in `API/Extensions/ResultExtensions.cs`
by **suffix convention**: `*.NotFound` → 404, `*.Conflict` / `*.Overlapping`
→ 409, everything else → 400. Name new codes accordingly.

Responses are RFC 9457 `ProblemDetails` with an `errorCode` extension.

### Entities

- Aggregates inherit `AggregateRoot`; construction only through static
  factories returning `Result<T>`; `private` setters; `private` parameterless
  constructor with a `// EF Core` comment.
- Ids are `Guid.CreateVersion7()`, generated in the factory.
- Value objects are `sealed record` with `private init` and a `Create`
  factory, mapped with EF Core `ComplexProperty`.
- Behaviour lives on the aggregate (`Cancel()`, `Complete()`), returning
  `Result`. Never mutate state from a handler.
- Domain events are raised inside the aggregate via `RaiseDomainEvent`.
- Aggregates carry a `RowVersion` optimistic-concurrency token (configured for
  every `AggregateRoot` in `DientesLimpiosDbContext.OnModelCreating`), so a second
  writer working from a stale copy is refused instead of overwriting the first.
- Audit fields (`CreatedBy`, `CreatedDate`, `LastModifiedBy`, `LastModifiedDate`)
  come from `IAuditable`: readable on the aggregate, with `private` setters and
  no setters on the interface, so neither an aggregate reference nor a cast can
  forge the trail. `AuditableEntitiesInterceptor` writes them through the change
  tracker (`entry.Property(nameof(IAuditable.CreatedDate)).CurrentValue = ...`),
  which reaches a private setter — do not reopen them to public "so the
  interceptor can set them". Nothing else, in any layer, writes them.
- Aggregates reference other aggregates **by id only** — `Appointment` holds
  `PatientId`, `DentistId` and `OfficeId` and no navigation properties. EF learns
  the relationships from `AppointmentConfig` through the id-only overload
  (`builder.HasOne<Patient>().WithMany().HasForeignKey(a => a.PatientId)`), which
  keeps the foreign keys and their `Restrict` behaviour without putting another
  aggregate on the entity. Do not add a navigation to serve a DTO; join instead.

### Use cases

One folder per use case under `Application/UseCases/<Aggregate>/{Commands,Queries}/<Name>/`
containing the command/query, its handler, its validator, and its DTO.
Handlers are primary-constructor classes implementing
`IRequestHandler<TRequest, TResponse>`, discovered by Scrutor assembly
scanning — no manual DI registration needed.

### Read side (appointments)

An appointment read never loads the aggregate to copy a name off a related one.
The handler takes `IApplicationDbContext` and projects with joins on the id
columns straight into its DTO, so three strings cost three joined columns instead
of three materialised aggregates:

```csharp
from a in db.Appointments.ApplyFilter(request).OrderBy(a => a.TimeInterval.Start)
join p in db.Patients on a.PatientId equals p.Id
join d in db.Dentists on a.DentistId equals d.Id
join o in db.Offices  on a.OfficeId  equals o.Id
select new AppointmentListDTO { /* … */ Patient = p.Name, Dentist = d.Name, Office = o.Name }
```

- There is no `MapperExtensions` under `UseCases/Appointments/` any more: the
  `select new` **is** the mapping. `AppointmentCreatedEmailHandler`,
  `AppointmentCancelledEmailHandler` and `SendAppointmentRemindersHandler` build
  their notification DTOs the same way, which is why none of them needs the
  repository.
- The filter lives once, in
  `UseCases/Appointments/Utilities/AppointmentQueryExtensions.ApplyFilter`, shared
  by `GetAppointmentListHandler` and `SendAppointmentRemindersHandler`. It returns
  `IQueryable` and never orders or materialises — each caller adds its own
  `OrderBy` and projection. Forgetting the `OrderBy` is the easy mistake.
- Inner joins are correct here because the foreign keys are non-nullable and
  `Restrict`ed, so a parent row cannot be missing.
- Commands are the other half of the rule: they load the tracked aggregate through
  `IAppointmentRepository.GetById` (no `Include`, no `AsNoTracking` — both handlers
  mutate and save) and change it through its own methods.

### Mediator

`SimpleMediator` replaced MediatR after its commercial licence change. It
validates via FluentValidation, short-circuits to `Result.Failure(ValidationError)`,
then dispatches by reflection. It has **no pipeline behaviours** — logging is
currently duplicated in every handler. If you add cross-cutting concerns,
propose a pipeline rather than copying more boilerplate.

### Persistence

Cross-cutting persistence behaviour goes in `SaveChangesInterceptor`s
(`AuditableEntitiesInterceptor`, `InsertOutboxMessagesInterceptor`), not in
`DbContext` overrides. Entity configuration goes in
`Persistence/Configurations/*Config.cs`, applied by
`ApplyConfigurationsFromAssembly`.

Command handlers persist with `db.SaveChangesAsResult(ct)`, which turns a
`DbUpdateConcurrencyException` into `Result.Failure(DomainErrors.Concurrency.Conflict)`
— a 409 by the error-code suffix convention — instead of letting it escape as a
500. When a mediator pipeline exists, that `catch` moves there and the helper goes.

### Domain events and the outbox

Aggregates raise events with `RaiseDomainEvent`. `InsertOutboxMessagesInterceptor`
turns them into `OutboxMessages` rows inside the same `SaveChanges`, so an event
commits — or rolls back — with the aggregate, and nothing is dispatched during the
request. `OutboxProcessorJob` polls on the interval set by its `PollingInterval`
constant; `OutboxProcessor` claims a batch with `UPDLOCK, READPAST, ROWLOCK` (so
several API instances never take the same row), dispatches each message in its
own DI scope, and records `ProcessedOnUtc` or `AttemptCount` + `Error`.

What this demands of new code:

- Event handlers must be **idempotent** and must let exceptions **propagate**.
  Swallowing one tells the processor the message succeeded, which is the bug the
  outbox exists to prevent.
- Event payloads are serialised as JSON, so every property needs an accessible
  setter (`init`, not `get`-only) or it comes back with a fresh value.
- `OutboxSerializer` resolves an event by its short type name against the Domain
  assembly; two events with the same name in different namespaces fail at startup.
- Delivery markers (`Appointment.ConfirmationSentAtUtc`, written by
  `AppointmentCreatedEmailHandler`, and `Appointment.CancellationSentAtUtc`, written
  by `AppointmentCancelledEmailHandler`) are written with `ExecuteUpdateAsync`,
  bypassing both the concurrency token and the aggregate. They are not part of the
  state machine, and a token veto would mean the email gets sent again on the
  retry. This is the one sanctioned exception to "never mutate state from a
  handler"; `Appointment.MarkConfirmationSent` stays as the in-memory invariant and
  has no production caller. `CancellationSentAtUtc` has no such method: nothing in
  the domain sets it, so tests cover it through the outbox integration tests.
- Events take `OccurredOnUtc` as a constructor parameter, supplied from the
  aggregate method's `nowUtc` argument (see `Appointment.Create`). Domain never
  reads the clock.

### Time

**Convention: every `DateTime` inside the application is UTC.** A local clock time
exists only where a person reads it. The convention is enforced at each boundary:

| Boundary | Enforced by |
|---|---|
| JSON in and out | `API/Json/UtcDateTimeJsonConverter` (registered in `Program.cs`): accepts only ISO 8601 with an offset or `Z`, so a zone-less value is a 400, and always writes `Z` |
| Query-string filters | ASP.NET Core binds offset and `Z` values to UTC (`AdjustToUniversal`); `GetAppointmentListQueryValidator` rejects zone-less ones |
| Database | `Persistence/Converters/UtcDateTimeConverter`, applied to every `DateTime` and `DateTime?` in `DientesLimpiosDbContext.ConfigureConventions`: throws on a non-UTC write and marks every read as UTC |
| Emails | `Infrastructure/Notifications/AppointmentDateFormatter`: the only place a stored instant becomes a clinic clock time |

Reading "now":

- Non-test code never calls `DateTime.UtcNow` or `DateTime.Now`. It takes the
  injected `TimeProvider`, registered once in `ApplicationServiceRegistration`.
- **Domain** stays free of the abstraction and receives the instant as a
  parameter (`Appointment.Create(..., nowUtc)`, `MarkConfirmationSent(nowUtc)`).
- **Validators** read the clock inside a lambda
  (`GreaterThan(_ => timeProvider.GetUtcNow().UtcDateTime)`). A plain value is
  captured once, when the validator is constructed.
- **Waits** use the overloads that take the provider, such as
  `Task.Delay(delay, timeProvider, ct)`. The plain overloads use the real clock,
  so `FakeTimeProvider.Advance` could never release them in a test.
- **"Today" and "tomorrow" are days at the clinic**, not UTC days. Convert with
  `ClinicOptions.TimeZoneId`, then back to UTC for queries. See
  `SendAppointmentRemindersHandler`.

What this demands of new code:

- A new `DateTime` property needs no mapping work, since the EF convention covers
  it, but whatever sets it must produce `Kind=Utc`
  (`timeProvider.GetUtcNow().UtcDateTime`, `TimeZoneInfo.ConvertTimeToUtc`).
  Anything else fails at `SaveChanges`.
- A new place that shows a time to a person converts with
  `ClinicOptions.TimeZoneId`, as `AppointmentDateFormatter` does.
- `DateTimeOffset` was considered and not adopted: the offset that matters is the
  clinic's, not the caller's. Do not mix the two types.

In tests, `Substitute.For<TimeProvider>()` is enough when the code only reads a
fixed instant. Use `FakeTimeProvider` only when the code under test waits on
time (see `AppointmentReminderJobTests`).

### Tests

Arrange/Act/Assert with comments, FluentAssertions (`.Should()`), NSubstitute
for mocks, `MockQueryable.NSubstitute` for `DbSet`. Naming:
`Method_Scenario_ExpectedOutcome`. Domain rules are tested in
`Tests/Domain/`, not through handlers.

---

## Known issues — do not "fix" silently, and do not replicate

An architecture review (August 2026) found these; the list was re-verified
against the code on 16 September 2026. If you touch adjacent code, flag them; if
asked to fix one, write the failing test first.

**Structural**

- Data access is still mixed, but less so: the appointment read paths are all
  projections and the appointment commands load the aggregate through
  `IAppointmentRepository`, which is the intended split. What remains is
  `CreateAppointmentHandler`, which uses both (existence checks on
  `IApplicationDbContext`, the insert through the repository), and the dentist,
  office and patient queries, which still load entities and map them with `ADto`.
  Preferred direction: commands through repositories/aggregates, queries
  projecting straight to DTOs with `AsNoTracking().Select(...)`.
- Validation is duplicated across API DataAnnotations (`API/DTOs/**`),
  FluentValidation validators and the domain factories, with divergent rules.
- Indexes are the EF Core defaults for foreign keys plus the filtered index on
  `OutboxMessages`; the appointment-overlap query has no covering index.
- The delete handlers (`DeleteDentist`, `DeleteOffice`, `DeletePatient`) still
  call `db.SaveChangesAsync` directly, so a concurrency conflict on a delete
  surfaces as a 500 rather than a 409. The `Create*` handlers do the same, which
  is harmless: an insert cannot conflict.
- The concurrency token is never exposed to clients. A client that reads, waits
  and writes back still wins, because the handler reloads inside the request.
  Closing that needs the version as an `ETag` with `If-Match`.
- `AppointmentReminderJob` checks the clinic clock once an hour and sends when
  the hour is 8. While the process stays up it fires once a day, but at an
  imprecise minute (anywhere from 08:00 to 08:59, depending on start time). A
  restart during that hour sends the reminders twice, and downtime covering the
  whole hour skips the day. A fix needs persisted "last run" state;
  `AppointmentReminderJobTests` already drives the schedule with `FakeTimeProvider`.
- All offices share one time zone (`ClinicOptions.TimeZoneId`). Per-office zones
  would need `Office.TimeZoneId`, reminders computed per office, and emails
  formatted in each office's zone.
- `Program.cs` catches every exception and logs "Web API terminated
  unexpectedly!", including the `HostAbortedException` that `dotnet ef` uses to
  stop the host after reading the model. That fatal line during `dotnet ef`
  commands is noise, not a failure.
- `SimpleMediator` still dispatches requests by reflection (`GetMethod` +
  `Invoke`). `DomainEventDispatcher` no longer does: it uses a cached generic
  wrapper, which is the technique to copy when reworking the mediator.
- `.github/workflows/` is empty — there is no CI.

**The outbox is implemented — know its limits**

`Persistence/Outbox/`, `InsertOutboxMessagesInterceptor` and `OutboxProcessorJob`
implement it. Storing an event with its aggregate is atomic, but delivery is not
exactly-once, so do not describe it that way:

- Delivery is **at-least-once**. `AppointmentCreatedEmailHandler` and
  `AppointmentCancelledEmailHandler` are idempotent through
  `Appointment.ConfirmationSentAtUtc` and `Appointment.CancellationSentAtUtc`,
  which narrow, but cannot close, the window between sending an email and
  recording that it was sent.
- A message stops being retried after `OutboxProcessor.MaxAttempts` (5) and stays
  in the table with `Error` set. There is no backoff between attempts, no
  dead-letter view and no alerting.
- `ProcessBatch` holds one transaction open for a whole batch, so a hung SMTP
  call holds it too (`SmtpClient.Timeout` defaults to 100 s).

---

## Naming migration in progress

These Spanish remnants exist. Do not add more; renaming them is welcome when
you are already editing the file, as an explicit, separate change:

`Paginar` → `Paginate` (`Persistence/Utilities/IQueryableExtensions.cs` and its
two callers) · `ADto` → projection or `ToDto` (12 sites, all in the dentist,
office and patient queries — the four appointment mappers were replaced by
projections, not renamed) ·
`AgregarServicesDeX` → `AddXServices` (8 sites) · policy `"esadmin"` → `"Admin"`
(`Program.cs`, `IdentityServiceRegistration`, `TestAuthHandler`) ·
`"Validacion.General"` → `"Validation.General"` (`ValidationError`) ·
header `total-number-of-records` → `X-Total-Count` · `PagedDTO.Elements` → `Items`.

Already done — do not re-report these: `Pagina` / `RegistrosPorPagina` →
`Page` / `RecordsPerPage`, `Elementos` → `Elements`, and the FluentValidation
messages, which are all English now.

The README was rewritten on 16 September 2026 and describes the current code
(custom mediator, hand-written mappers, outbox). It is no longer stale — keep it
in step when you change the architecture.

---

## Working style

- Explain the reasoning before applying a change; proceed incrementally rather
  than delivering a large refactor in one step.
- Prefer the smallest change that fixes the actual problem. Do not restructure
  folders, rename broadly, or upgrade packages as a side effect of an
  unrelated task.
- When a fix has design alternatives, name them and their trade-offs instead of
  picking one silently.
- Run `dotnet build` and the relevant tests before reporting a task complete.
  If they were not run, say so.
- Do not create documentation files unless asked.