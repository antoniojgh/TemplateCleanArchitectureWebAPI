using Asp.Versioning;
using DientesLimpios.API.ExceptionHandlers;
using DientesLimpios.API.Jobs;
using DientesLimpios.Application;
using DientesLimpios.Identity;
using DientesLimpios.Identity.Models;
using DientesLimpios.Infrastructure;
using DientesLimpios.Persistence;
using Microsoft.AspNetCore.Mvc.Authorization;
using Serilog;


// Setup the initial logger with Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Web API...");
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog to the Host
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Add services to the container.

    builder.Services.AddControllers(opciones =>
    {
        // Adds a global authorization filter to protect all endpoints by default
        opciones.Filters.Add(new AuthorizeFilter("esadmin"));
    });

    // Dependency injection for Application, Persistence and Infrastructure layers
    builder.Services.AgregarServicesDeApplication();
    builder.Services.AgregarServicesDePersistence(builder.Configuration);
    builder.Services.AgregarServicesDeInfrastructure(builder.Configuration);
    builder.Services.AgregarServicesDeIdentity(builder.Configuration);

    // Add the background service for appointment reminders
    builder.Services.AddHostedService<AppointmentReminderJob>();

    // API versioning configuration
    builder.Services.AddApiVersioning(options =>
    {
        // 1. Set the default version to 1.0
        options.DefaultApiVersion = new ApiVersion(1, 0);

        // 2. If the client doesn't specify a version, use the default (1.0)
        // This is critical to avoid breaking existing clients that don't send a version yet.
        options.AssumeDefaultVersionWhenUnspecified = true;

        // 3. Report the supported versions in the HTTP response headers (api-supported-versions)
        options.ReportApiVersions = true;

        // 4. Read the version from the URL (e.g., /api/v1/appointments)
        // You can also configure it to read from Header or QueryString here if preferred.
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc(); // Add MVC support for versioning

    // Global exception handling configuration
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Add Request Logging Middleware
    app.UseSerilogRequestLogging();

    app.MapIdentityApi<User>();

    app.UseExceptionHandler();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Web API terminated unexpectedly!");
}
finally
{
    Log.CloseAndFlush();
}
