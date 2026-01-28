# DientesLimpios - Clean Architecture Web API Template

**DientesLimpios** is a robust .NET Web API template designed to demonstrate **Clean Architecture** and **Domain-Driven Design (DDD)** principles. It manages a dental clinic domain (Patients, Dentists, Offices, and Appointments) while enforcing strict separation of concerns, ensuring maintainability, testability, and scalability.

## 🏗 Architecture

The solution follows the "Clean Architecture" onion structure, where dependencies flow inward.

* **Core (Domain & Application)**: The heart of the system. It contains business logic and use cases, with no dependencies on external libraries or databases.
* **Infrastructure**: Implements interfaces defined in the Core (e.g., Email Services, Identity).
* **Persistence**: Handles database access using Entity Framework Core.
* **API**: The entry point (Presentation Layer) communicating via RESTful endpoints.

## 🚀 Technologies

* **Framework**: .NET (Configured for `net10.0` in csproj, adaptable to .NET 8/9)
* **ORM**: Entity Framework Core
* **Mediator Pattern**: MediatR (for CQRS)
* **Validation**: FluentValidation
* **Mapping**: AutoMapper
* **Authentication**: ASP.NET Core Identity
* **Logging**: Serilog (Structured Logging)
* **API Versioning**: Asp.Versioning.Mvc
* **Testing**: xUnit, NSubstitute
* **Documentation**: Microsoft.AspNetCore.OpenApi (Swagger)

## ✨ Key Features

* **CQRS (Command Query Responsibility Segregation)**: Operations are split into **Commands** (Writes) and **Queries** (Reads) using MediatR handlers.
* **Structured Logging**: Integrated **Serilog** to capture rich, structured logs across the application. Configured for Console and daily rolling File outputs, providing detailed context for debugging global exceptions, background tasks, and external integrations.
* **Rich Domain Model**: Entities like `Cita` (Appointment) enforce business rules via private setters, constructor validation, and Value Objects (`Email`, `IntervaloDeTiempo`).
* **Validation Pipeline**: Automatic request validation using MediatR Behaviors (`ValidationBehavior`) before reaching the handler.
* **Global Exception Handling**: Custom Middleware to catch exceptions and return standardized API error responses, now fully integrated with Serilog for stack trace capture.
* **Background Jobs**: Hosted services for background tasks, such as sending appointment reminders (`RecordatorioCitasJob`).
* **Auditable Entities**: Automatic tracking of creation/modification dates via `EntidadAuditable`.
* **API Versioning**: Supports versioned endpoints (e.g., `/api/v1/citas`).

## 📂 Project Structure

```text
├── src
│   ├── DientesLimpios.API            # Entry point, Controllers, Middlewares, IoC, Logging Config
│   ├── DientesLimpios.Aplicacion     # Use Cases (CQRS), DTOs, Interfaces, Validators
│   ├── DientesLimpios.Dominio        # Entities, Enums, Value Objects, Domain Exceptions
│   ├── DientesLimpios.Infraestructura# External services (Email), 3rd party integrations
│   ├── DientesLimpios.Identidad      # Identity logic (Auth, Users)
│   └── DientesLimpios.Persistencia   # DbContext, Repositories, Migrations
├── tests
│   └── DientesLimpios.Tests          # Unit and Integration tests (xUnit)

```

## 🛠 Getting Started

### Prerequisites

* [.NET SDK](https://dotnet.microsoft.com/download) (Version matching `TargetFramework` in csproj)
* SQL Server (LocalDB or Docker container)

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/yourusername/templatecleanarchitecturewebapi.git
cd templatecleanarchitecturewebapi

```


2. **Configure Database Connection**
Update `appsettings.json` in the `DientesLimpios.API` project with your connection string:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DientesLimpiosDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}

```


3. **Apply Migrations**
Create the database and apply the schema:
```bash
dotnet tool install --global dotnet-ef
cd DientesLimpios.API
dotnet ef database update --project ../DientesLimpios.Persistencia/DientesLimpios.Persistencia.csproj --startup-project .

```


4. **Run the Application**
```bash
dotnet run

```



## 🧪 Testing

The solution includes a comprehensive test suite using **xUnit** and **NSubstitute** for mocking.

To run the tests:

```bash
dotnet test

```

* **Domain Tests**: Verify business logic and invariants (e.g., `CitaTests`, `IntervaloDeTiempoTests`).
* **Application Tests**: Verify Use Cases and Handlers.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.txt](https://www.google.com/search?q=LICENSE.txt) file for details.
