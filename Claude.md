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
dotnet ef migrations add <Name> --project DientesLimpios.Persistence --startup-project DientesLimpios.API
dotnet ef migrations add <Name> --project DientesLimpios.Identity   --startup-project DientesLimpios.API --context DientesLimpiosIdentityDbContext
dotnet ef database update --project DientesLimpios.Persistence --startup-project DientesLimpios.API
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
(`AuditableEntitiesInterceptor`, `DispatchDomainEventsInterceptor`), not in
`DbContext` overrides. Entity configuration goes in
`Persistence/Configurations/*Config.cs`, applied by
`ApplyConfigurationsFromAssembly`.

### Tests

Arrange/Act/Assert with comments, FluentAssertions (`.Should()`), NSubstitute
for mocks, `MockQueryable.NSubstitute` for `DbSet`. Naming:
`Method_Scenario_ExpectedOutcome`. Domain rules are tested in
`Tests/Domain/`, not through handlers.

---

## Known issues — do not "fix" silently, and do not replicate

An architecture review (August 2026) found these. If you touch adjacent code,
flag them; if asked to fix one, write the failing test first.

**Correctness**

1. `SendAppointmentRemindersHandler` filters only on `StartDate >= tomorrow`
   with no upper bound — it reminds about *every* future appointment daily.
2. `GetDentistListHandler` / `GetPatientListHandler` compute `Total` with an
   unfiltered `CountAsync()`, so paginated totals are wrong.
3. `CreateAppointmentHandler` checks overlap then inserts with no transaction
   or lock — concurrent double-booking is possible.
4. All three `Appointments` foreign keys cascade-delete: removing a dentist
   destroys their appointment history.
5. An unknown `PatientId`/`DentistId`/`OfficeId` produces a `DbUpdateException`
   → 500 with the raw SQL error text in `Detail`.

**Structural**

- There is **no Transactional Outbox on this branch**. Domain events are
  dispatched in-process after commit; a failed email is lost. Do not describe
  the project as having an outbox.
- `IUnitOfWork` / `EFCoreUnitOfWork` are registered but never used.
- `Application/Exceptions/NotFoundException` and `ValidationException` are
  dead code.
- Data access is inconsistent: some handlers use `IApplicationDbContext`,
  some use repositories, `GetDentistListHandler` uses both. Preferred
  direction: commands through repositories/aggregates, queries projecting
  straight to DTOs with `AsNoTracking().Select(...)`.
- Validation is duplicated across API DataAnnotations, FluentValidation, and
  the domain, with divergent rules.
- No indexes beyond primary keys; no concurrency tokens.
- `DateTime.UtcNow` is called directly in ~8 places; there is no
  `TimeProvider` and no stated UTC convention.
- `.github/workflows/` is empty — there is no CI.

---

## Naming migration in progress

These Spanish remnants exist. Do not add more; renaming them is welcome when
you are already editing the file, as an explicit, separate change:

`Pagina` → `Page` · `RegistrosPorPagina` → `PageSize` · `Elementos` → `Items` ·
`Paginar` → `Paginate` · `ADto` → projection or `ToDto` ·
`AgregarServicesDeX` → `AddXServices` · policy `"esadmin"` → `"Admin"` ·
header `cantidad-total-registros` → `X-Total-Count` ·
`"Validacion.General"` → `"Validation.General"` · Spanish validator messages.

The README is stale (it still describes MediatR, AutoMapper, and the old
Spanish project names) — do not use it as a source of truth about the code.

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