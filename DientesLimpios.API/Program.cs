using DientesLimpios.API.Jobs;
using DientesLimpios.API.Middlewares;
using DientesLimpios.Aplicacion;
using DientesLimpios.Infraestructura;
using DientesLimpios.Persistencia;
using DientesLimpios.Identidad;
using DientesLimpios.Identidad.Modelos;
using Microsoft.AspNetCore.Mvc.Authorization;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(opciones =>
{
    // Agregar un filtro global de autorización para proteger todos los endpoints por defecto
    opciones.Filters.Add(new AuthorizeFilter("esadmin"));
});

// Inyección de dependencias de la capa de Aplicación, Persistencia e Infraestructura
builder.Services.AgregarServiciosDeAplicacion();
builder.Services.AgregarServiciosDePersistencia();
builder.Services.AgregarServiciosDeInfraestructura();
builder.Services.AgregarServiciosDeIdentidad();

// Agregar el servicio de trabajo en segundo plano para el recordatorio de citas
builder.Services.AddHostedService<RecordatorioCitasJob>();

// Configuración de versionado de API
builder.Services.AddApiVersioning(options =>
{
    // 1. Set the default version to 1.0
    options.DefaultApiVersion = new ApiVersion(1, 0);

    // 2. If the client doesn't specify a version, use the default (1.0)
    // This is critical to avoid breaking existing clients that don't send a version yet.
    options.AssumeDefaultVersionWhenUnspecified = true;

    // 3. Report the supported versions in the HTTP response headers (api-supported-versions)
    options.ReportApiVersions = true;

    // 4. Read the version from the URL (e.g., /api/v1/citas)
    // You can also configure it to read from Header or QueryString here if preferred.
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddMvc(); // Add MVC support for versioning


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapIdentityApi<Usuario>();

// Agregar el middleware personalizado para el manejo de excepciones
app.UseManejadorExcepciones();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
