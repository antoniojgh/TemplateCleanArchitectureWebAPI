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
| Tests | xUnit, NSubstitute, FluentAssertions, NetArchTest, Testcontainers |
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

### Use cases

One folder per use case under `Application/UseCases/<Aggregate>/{Commands,Queries}/<Name>/`
containing the command/query, its handler, its validator, and its DTO.
Handlers are primary-constructor classes implementing
`IRequestHandler<TRequest, TResponse>`, discovered by Scrutor assembly
scanning — no manual DI registration needed.

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

### Domain events and the outbox

Aggregates raise events with `RaiseDomainEvent`. `InsertOutboxMessagesInterceptor`
turns them into `OutboxMessages` rows inside the same `SaveChanges`, so an event
commits — or rolls back — with the aggregate, and nothing is dispatched during the
request. `OutboxProcessorJob` polls every 5 s; `OutboxProcessor` claims a batch
with `UPDLOCK, READPAST, ROWLOCK` (so several API instances never take the same
row), dispatches each message in its own DI scope, and records `ProcessedOnUtc`
or `AttemptCount` + `Error`.

What this demands of new code:

- Event handlers must be **idempotent** and must let exceptions **propagate**.
  Swallowing one tells the processor the message succeeded, which is the bug the
  outbox exists to prevent.
- Event payloads are serialised as JSON, so every property needs an accessible
  setter (`init`, not `get`-only) or it comes back with a fresh value.
- `OutboxSerializer` resolves an event by its short type name against the Domain
  assembly; two events with the same name in different namespaces fail at startup.

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

- Data access is inconsistent: most handlers use `IApplicationDbContext`
  directly, the appointment use cases go through `IAppointmentRepository`, and
  `CreateAppointmentHandler` and `AppointmentCreatedEmailHandler` use both.
  Preferred direction: commands through repositories/aggregates, queries
  projecting straight to DTOs with `AsNoTracking().Select(...)`.
- Validation is duplicated across API DataAnnotations (`API/DTOs/**`),
  FluentValidation validators and the domain factories, with divergent rules.
- No optimistic-concurrency tokens (`rowversion`) on any aggregate. Indexes are
  the EF Core defaults for foreign keys plus the filtered index on
  `OutboxMessages`; the appointment-overlap query has no covering index.
- `DateTime.UtcNow` is still called directly in four non-test files
  (`CreateAppointmentCommandValidator`, `AppointmentCreatedEvent`,
  `AuditableEntitiesInterceptor`, `AppointmentRepository`). `TimeProvider` is
  registered in `ApplicationServiceRegistration` and used by the outbox path and
  the reminder use case — new code should take it rather than adding a fifth
  call site.
- `SimpleMediator` still dispatches requests by reflection (`GetMethod` +
  `Invoke`). `DomainEventDispatcher` no longer does: it uses a cached generic
  wrapper, which is the technique to copy when reworking the mediator.
- `.github/workflows/` is empty — there is no CI.

**The outbox is implemented — know its limits**

`Persistence/Outbox/`, `InsertOutboxMessagesInterceptor` and `OutboxProcessorJob`
implement it. Storing an event with its aggregate is atomic, but delivery is not
exactly-once, so do not describe it that way:

- Delivery is **at-least-once**. `AppointmentCreatedEmailHandler` is idempotent
  through `Appointment.ConfirmationSentAtUtc`, which narrows, but cannot close,
  the window between sending an email and recording that it was sent.
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
two callers) · `ADto` → projection or `ToDto` (20 sites) ·
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