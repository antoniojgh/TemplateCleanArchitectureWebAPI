# DientesLimpios — Phase A Migration

> **Audience:** Claude Code, executing in the DientesLimpios repository.
> **Goal:** Apply small, mechanical hygiene improvements that make the project look professional at the file level — central build properties, central package versions, the `IUnitOfwork` typo fix, connection-string externalization, DI extension consistency, and `ExcepcionNoEncontrado` enrichment.
> **Why this is first:** every later phase edits handler files, `csproj` files, or both. Doing the hygiene pass first means those edits land on a clean, normalized foundation. Doing it later means editing the same files twice.

---

## 0. Context and ground rules

Before writing any code, read this entire document. Do not start at Step 1 and improvise the rest.

**Architectural principles to respect throughout:**

1. **No behavioral changes.** Phase A is purely cosmetic at runtime. The application should behave identically before and after — same endpoints, same responses, same database. What changes is the *shape* of the project files, package management, and a few code-level cleanups.
2. **Be prepared for warnings to surface.** Adding `TreatWarningsAsErrors=true` and `AnalysisMode=All` will likely produce a wave of new build errors. This is a feature, not a bug. The guide tells you how to handle them.
3. **Spanish naming is the convention.** Folders, classes, properties, namespaces — match the existing codebase. The one exception is the `IUnitOfwork` typo fix; this guide uses `IUnitOfWork` (English, just fixing the typo) because that is what the document of reference suggests. If you prefer the Spanish form `IUnidadDeTrabajo`, do that instead — but be consistent with whichever you pick, because Phase B will reuse the same name.
4. **The architecture tests must keep passing.** Run `dotnet test --filter FullyQualifiedName~ArchitectureTests` after every step. Phase A should not affect them at all — if any rule fails, something went wrong.
5. **One commit per step.** Each step is designed to leave the project in a compiling, test-passing state. Commit after each one.
6. **Do NOT touch handlers, entities, or controllers in Phase A.** This phase is about project files, configuration, and one tiny exception class. Anything beyond that belongs to a later phase.

**Decision made for you (override before starting if you disagree):**

- **The `IUnitOfwork` rename uses the English form `IUnitOfWork`.** This matches the original document's wording. Phase B will reuse this name. If you prefer the Spanish form `IUnidadDeTrabajo`, change it consistently across both phases — but don't mix.

---

## 1. Add `Directory.Build.props` at the solution root

This file is automatically picked up by every `csproj` in the solution, and the properties cascade down. After adding it, you can remove duplicated properties from individual `csproj` files.

### 1.1 Create the file

File: `Directory.Build.props` (at the same level as the `.sln` file)

```xml
<Project>

  <PropertyGroup>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <AnalysisMode>All</AnalysisMode>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>

</Project>
```

### 1.2 Strip duplicates from individual `csproj` files

For each project file under `src/` (and `tests/` if present):

```xml
<!-- Before -->
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <Nullable>enable</Nullable>
  <ImplicitUsings>enable</ImplicitUsings>
</PropertyGroup>

<!-- After -->
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
</PropertyGroup>
```

Keep `<TargetFramework>` (it can vary per project), `<RootNamespace>`, `<AssemblyName>`, and any project-specific properties. Remove only the ones now defined globally: `Nullable`, `ImplicitUsings`, and `LangVersion` if it was set per-project.

### 1.3 Build and triage warnings

```bash
dotnet build --no-incremental
```

**This will likely fail the first time.** `TreatWarningsAsErrors=true` plus `AnalysisMode=All` turns dozens of analyzer warnings into errors. Common categories you will see:

| Warning | What to do |
|---|---|
| `CA1062`: validate non-null public arguments | Add `ArgumentNullException.ThrowIfNull(...)` at the top of public methods, or accept it project-wide via `.editorconfig` (see below). |
| `CA1707`: identifiers should not contain underscores | Usually triggered by test method names like `Should_DoX_When_Y`. Suppress for test projects via `.editorconfig`. |
| `CA1812`: avoid uninstantiated internal classes | Affects DTOs deserialized by reflection. Suppress per-class with `[SuppressMessage]` or globally via `.editorconfig`. |
| `CA1515`: types can be made internal | Genuinely useful — apply where it makes sense, suppress where it doesn't. |
| `CS1591`: missing XML doc on public member | Either write the docs or disable doc generation. For an API project, disabling is usually correct. |

**Recommended approach:** create a root `.editorconfig` (if one does not already exist) that downgrades the noisy rules. This keeps `AnalysisMode=All` on but tames the most annoying false positives:

File: `.editorconfig` (at the same level as the `.sln`)

```ini
root = true

[*.cs]
# Production code
dotnet_diagnostic.CA1062.severity = suggestion   # Validate arguments of public methods
dotnet_diagnostic.CA1812.severity = suggestion   # Avoid uninstantiated internal classes
dotnet_diagnostic.CA1515.severity = suggestion   # Types can be made internal
dotnet_diagnostic.CS1591.severity = none         # Missing XML comment

[**/*Tests/**.cs]
# Test code — relax further
dotnet_diagnostic.CA1707.severity = none         # Underscores in identifiers (test names)
dotnet_diagnostic.CA1822.severity = none         # Mark members as static
dotnet_diagnostic.CA2007.severity = none         # ConfigureAwait
```

After adding the `.editorconfig`:

```bash
dotnet build --no-incremental
```

If errors remain, fix them iteratively. **Do not turn off `TreatWarningsAsErrors` to make them go away.** The point of Phase A is to leave the project in a state where warnings cannot accumulate silently.

### 1.4 Verify

- `dotnet build` succeeds with no errors and no warnings.
- `dotnet test --filter FullyQualifiedName~ArchitectureTests` passes.

**Commit message:** `chore: add Directory.Build.props and .editorconfig for centralized build settings`

---

## 2. Add `Directory.Packages.props` for central package management

This pins every NuGet package version in one file, so individual `csproj` files reference packages without specifying versions. Updating a package becomes a one-file change instead of a hunt across the solution.

### 2.1 Inventory current package versions

Run a search to enumerate every `PackageReference` across the solution:

```bash
grep -rhE "<PackageReference Include=" src/ tests/ 2>/dev/null | sort -u
```

The output is a deduplicated list. **Action for Claude Code:** capture this list before continuing — you will need it for Step 2.2. If the same package appears with two different versions (drift), pick the highest version and note the drift in the commit message.

### 2.2 Create `Directory.Packages.props`

File: `Directory.Packages.props` (at the same level as the `.sln`)

```xml
<Project>

  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>

  <ItemGroup>
    <!-- One PackageVersion entry per unique package across the solution -->
    <!-- Versions taken from the inventory in Step 2.1 -->

    <!-- EF Core / Persistence -->
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="10.0.1" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.1" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.1" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.1" />

    <!-- ASP.NET Core / API -->
    <PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="10.0.1" />
    <PackageVersion Include="Asp.Versioning.Mvc" Version="8.1.1" />
    <PackageVersion Include="Asp.Versioning.Mvc.ApiExplorer" Version="8.1.1" />

    <!-- MediatR / AutoMapper -->
    <PackageVersion Include="MediatR" Version="14.0.0" />
    <PackageVersion Include="AutoMapper" Version="12.0.1" />
    <PackageVersion Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />

    <!-- FluentValidation -->
    <PackageVersion Include="FluentValidation" Version="12.1.1" />
    <PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="12.1.1" />

    <!-- Serilog -->
    <PackageVersion Include="Serilog.AspNetCore" Version="10.0.0" />
    <PackageVersion Include="Serilog.Sinks.Console" Version="6.0.0" />
    <PackageVersion Include="Serilog.Sinks.File" Version="6.0.0" />

    <!-- Identity / JWT -->
    <PackageVersion Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.1" />
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.1" />

    <!-- Test packages -->
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageVersion Include="xunit" Version="2.9.2" />
    <PackageVersion Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageVersion Include="NSubstitute" Version="5.3.0" />
    <PackageVersion Include="FluentAssertions" Version="6.12.2" />
    <PackageVersion Include="NetArchTest.Rules" Version="1.3.2" />

    <!-- Add anything else from the Step 2.1 inventory that is not yet listed above -->

  </ItemGroup>

</Project>
```

**Important:** the versions above are the ones the document of reference indicated. The actual inventory from Step 2.1 may differ — **use the inventory's versions, not the ones in this guide**. If two projects reference different versions of the same package, pick the highest one.

### 2.3 Strip versions from `csproj` files

For every `PackageReference` across the solution:

```xml
<!-- Before -->
<PackageReference Include="MediatR" Version="14.0.0" />

<!-- After -->
<PackageReference Include="MediatR" />
```

The `Version` attribute disappears. The reference itself stays — central package management does not remove the dependency, just the version specification.

### 2.4 Verify

```bash
dotnet restore
dotnet build --no-incremental
dotnet test
```

If a build error like `NU1010: PackageReference must have a version` appears, that package is missing from `Directory.Packages.props`. Add it.

**Commit message:** `chore: enable central package management via Directory.Packages.props`

---

## 3. Rename `IUnitOfwork` → `IUnitOfWork`

Use the IDE rename refactor, not search-and-replace, so all usages update transactionally.

### 3.1 Identify scope

```bash
grep -rn "IUnitOfwork" src/ tests/
```

The hits are typically:
- The interface declaration (`IUnitOfwork.cs`).
- The implementation class (`UnidadDeTrabajoEFCore.cs` likely says `: IUnitOfwork`).
- Every handler that injects it.
- The DI registration in `RegistroDeServiciosDePersistencia.cs`.
- Possibly a few test fixtures.

### 3.2 Perform the rename

Open the interface file, place the cursor on the type name, and use the IDE's rename refactor (`F2` in most IDEs). Type the new name `IUnitOfWork`. Confirm. The IDE updates every reference.

If you are doing this without an IDE, use a tool that updates code-aware:

```bash
# Last-resort search and replace
find src/ tests/ -name "*.cs" -exec sed -i 's/IUnitOfwork/IUnitOfWork/g' {} +
```

Then **rename the file itself**:

```bash
git mv src/DientesLimpios.Aplicacion/Interfaces/Persistencia/IUnitOfwork.cs \
       src/DientesLimpios.Aplicacion/Interfaces/Persistencia/IUnitOfWork.cs
```

### 3.3 Verify

```bash
grep -rn "IUnitOfwork" src/ tests/   # should return nothing
dotnet build --no-incremental
dotnet test
```

**Commit message:** `chore: rename IUnitOfwork → IUnitOfWork (typo fix)`

---

## 4. Move connection strings out of code into `appsettings.json`

The persistence and identity DI extensions currently hardcode `UseSqlServer("name=DientesLimpiosConnectionString")`. Replace with proper `IConfiguration` lookup.

### 4.1 Add the connection string to `appsettings.json`

File: `src/DientesLimpios.API/appsettings.json`

Add (or update) the `ConnectionStrings` section:

```json
{
  "ConnectionStrings": {
    "DientesLimpios": "Server=(localdb)\\MSSQLLocalDB;Database=DientesLimpios;Trusted_Connection=True;TrustServerCertificate=True;"
  }
  // ... existing settings ...
}
```

**Action for Claude Code:** check what value the connection string actually had. If it was already in `appsettings.json` under a different key name, use that key consistently below — but the *recommended* key is `DientesLimpios` (a single, plain name).

If `appsettings.Development.json` exists, mirror the connection string there with development-appropriate settings.

### 4.2 Update `RegistroDeServiciosDePersistencia.cs`

```csharp
// Before
public static IServiceCollection AgregarServiciosDePersistencia(
    this IServiceCollection services)
{
    services.AddDbContext<DientesLimpiosDbContext>(options =>
        options.UseSqlServer("name=DientesLimpiosConnectionString"));
    // ...
    return services;
}

// After
public static IServiceCollection AgregarServiciosDePersistencia(
    this IServiceCollection services,
    IConfiguration configuration)
{
    var cadenaConexion = configuration.GetConnectionString("DientesLimpios")
        ?? throw new InvalidOperationException(
            "La cadena de conexión 'DientesLimpios' no está configurada.");

    services.AddDbContext<DientesLimpiosDbContext>(options =>
        options.UseSqlServer(cadenaConexion));
    // ...
    return services;
}
```

### 4.3 Update `RegistroDeServiciosDeIdentidad.cs`

Same treatment — accept `IConfiguration`, look up the same connection string by name. **Do not duplicate the string** — both extensions read it from configuration so changing it in `appsettings.json` updates both.

### 4.4 Update the call sites in `Program.cs`

```csharp
// Before
builder.Services.AgregarServiciosDePersistencia();
builder.Services.AgregarServiciosDeIdentidad();

// After
builder.Services.AgregarServiciosDePersistencia(builder.Configuration);
builder.Services.AgregarServiciosDeIdentidad(builder.Configuration);
```

### 4.5 Verify

```bash
dotnet build
dotnet run --project src/DientesLimpios.API
```

The application should start and connect to the database successfully. Hit any endpoint that touches the database (e.g. `GET /api/v1/citas`) to confirm.

**Commit message:** `refactor: move connection string from code to appsettings.json`

---

## 5. Make `AgregarServiciosDeIdentidad` return `IServiceCollection`

The other three `Agregar*` extensions return `IServiceCollection`, so they can be chained in `Program.cs`. `AgregarServiciosDeIdentidad` is the odd one out — it returns `void`. Fix it.

### 5.1 Update the signature

File: `RegistroDeServiciosDeIdentidad.cs` (or wherever it lives)

```csharp
// Before
public static void AgregarServiciosDeIdentidad(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // ... body ...
}

// After
public static IServiceCollection AgregarServiciosDeIdentidad(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // ... body unchanged ...
    return services;
}
```

### 5.2 Update the call site in `Program.cs`

Now the four registrations can chain:

```csharp
builder.Services
    .AgregarServiciosDeAplicacion()
    .AgregarServiciosDePersistencia(builder.Configuration)
    .AgregarServiciosDeIdentidad(builder.Configuration)
    .AgregarServiciosDeApi(builder.Configuration);
```

(Adjust the chain order to match your actual project — the exact set of `Agregar*` extensions and their parameter signatures.)

### 5.3 Verify

```bash
dotnet build
dotnet test
```

**Commit message:** `refactor: AgregarServiciosDeIdentidad returns IServiceCollection for chaining`

---

## 6. Enrich `ExcepcionNoEncontrado` with entity context

The exception is currently empty — no entity name, no identifier, no message. Even though the Result pattern (Phase C) will replace it for *expected* not-found cases, the exception still needs to exist as a safety net and should carry useful context if it gets thrown.

### 6.1 Update the class

File: `DientesLimpios.Aplicacion/Excepciones/ExcepcionNoEncontrado.cs` (or wherever it lives)

```csharp
namespace DientesLimpios.Aplicacion.Excepciones;

public class ExcepcionNoEncontrado : Exception
{
    public string TipoEntidad { get; }
    public object Identificador { get; }

    public ExcepcionNoEncontrado(string tipoEntidad, object id)
        : base($"{tipoEntidad} con id '{id}' no encontrado.")
    {
        TipoEntidad = tipoEntidad;
        Identificador = id;
    }

    // Keep a parameterless constructor only if existing code currently calls it
    // without arguments. Otherwise, omit it — forcing callers to supply context
    // is the whole point of this enrichment.
}
```

### 6.2 Update existing throw sites

```bash
grep -rn "throw new ExcepcionNoEncontrado()" src/
```

For each hit, supply the entity type and the id:

```csharp
// Before
throw new ExcepcionNoEncontrado();

// After
throw new ExcepcionNoEncontrado(nameof(Paciente), query.Id);
```

**Action for Claude Code:** the `nameof(Paciente)` form is preferred because it survives renames. Use it consistently.

### 6.3 Verify

```bash
dotnet build
dotnet test
```

If existing tests asserted on the exception message and the message has changed, update the assertions.

**Commit message:** `refactor: ExcepcionNoEncontrado carries entity type and identifier`

---

## 7. Final verification

### 7.1 Confirm the project still compiles cleanly

```bash
dotnet build --no-incremental
```

Zero errors, zero warnings.

### 7.2 Confirm all tests pass

```bash
dotnet test
```

Including architecture tests:

```bash
dotnet test --filter FullyQualifiedName~ArchitectureTests --logger "console;verbosity=detailed"
```

### 7.3 Smoke test the running application

```bash
dotnet run --project src/DientesLimpios.API
```

Hit a couple of endpoints to confirm the connection-string move did not break anything:
- `GET /api/v1/citas` (or whatever the listing endpoint is) — should return data or an empty list, not a 500.
- `POST /api/v1/citas` (with a valid payload) — should succeed.

### 7.4 Inspect the final state of the solution root

After Phase A, the root of the solution should contain (alongside the `.sln` and `.gitignore`):

```
Directory.Build.props
Directory.Packages.props
.editorconfig
```

Each `csproj` file:
- Has no `<Nullable>`, `<ImplicitUsings>`, or `<LangVersion>` properties.
- Has `<PackageReference>` entries with no `Version` attribute.

---

## 8. Final state — what Phase A has delivered

| Concern | Before | After |
|---|---|---|
| Build settings | Duplicated in every `csproj` | Centralized in `Directory.Build.props` |
| Package versions | Drift across `csproj` files (some 10.0.1, some older) | Single source of truth in `Directory.Packages.props` |
| Warning policy | Loose — warnings accumulate silently | Strict — `TreatWarningsAsErrors` plus `AnalysisMode=All` |
| `IUnitOfwork` typo | Present | Fixed to `IUnitOfWork` |
| Connection string | Hardcoded in DI extension | Read from `appsettings.json` via `IConfiguration` |
| `AgregarServiciosDeIdentidad` | Returns `void` | Returns `IServiceCollection` for chaining |
| `ExcepcionNoEncontrado` | Empty — no information | Carries `TipoEntidad` and `Identificador`, has a meaningful message |
| Behavior | Same as before | Same as before |

**What Phase A has NOT done** (these come later):

- No change to handlers, entities, controllers, repositories, or value objects.
- No persistence-layer refactor — that is Phase B.
- No Result pattern — that is Phase C.
- No domain events, integration tests, or strongly-typed IDs.

---

## 9. Constraint checklist for Claude Code

Before declaring Phase A complete, verify ALL of these:

- [ ] `Directory.Build.props` exists at the solution root and contains `TreatWarningsAsErrors`, `AnalysisMode`, `Nullable`, `ImplicitUsings`.
- [ ] `Directory.Packages.props` exists at the solution root and lists every package referenced anywhere in the solution.
- [ ] `.editorconfig` exists (either pre-existing or new) with severity downgrades for the noisiest analyzer rules.
- [ ] No `csproj` file contains `<Nullable>`, `<ImplicitUsings>`, or `<LangVersion>` properties (they cascade from `Directory.Build.props`).
- [ ] No `<PackageReference>` element anywhere has a `Version` attribute.
- [ ] `grep -rn "IUnitOfwork" src/ tests/` returns nothing.
- [ ] `IUnitOfWork.cs` is the file name (renamed via `git mv`).
- [ ] No connection string appears as a hardcoded literal in any `.cs` file. Search: `grep -rn "Server=" src/` and `grep -rn "name=Dientes" src/`.
- [ ] `appsettings.json` contains the connection string under `ConnectionStrings:DientesLimpios`.
- [ ] Both `AgregarServiciosDePersistencia` and `AgregarServiciosDeIdentidad` accept `IConfiguration` and use `configuration.GetConnectionString("DientesLimpios")`.
- [ ] `AgregarServiciosDeIdentidad` returns `IServiceCollection`.
- [ ] `Program.cs` chains the four `Agregar*` calls fluently.
- [ ] `ExcepcionNoEncontrado` has `TipoEntidad` and `Identificador` properties and a meaningful message.
- [ ] Every existing `throw new ExcepcionNoEncontrado(...)` call passes the entity type and id.
- [ ] `dotnet build --no-incremental` succeeds with zero errors and zero warnings.
- [ ] `dotnet test` passes for all test projects, including architecture tests.
- [ ] Manual smoke test of the running application confirms endpoints still work.

---

## 10. Out of scope (do NOT do these as part of Phase A)

These belong to other phases and must NOT be done in this PR:

- Persistence-layer refactor (`IApplicationDbContext`, removing the generic repository, real Unit of Work, auditing interceptor) — that is **Phase B**.
- Result pattern, `IExceptionHandler`, `ProblemDetails` — that is **Phase C**.
- Domain events — that is Phase D.
- Integration tests with Testcontainers — that is Phase F.
- Strongly-Typed IDs — that is Phase G.
- MediatR / AutoMapper migration — that is Phase E.

If you find yourself wanting to clean up something that is not on this list, stop. Phase A is intentionally narrow. The point is fast, mechanical wins that leave the project file structure in a known-clean state — anything else dilutes the diff.
