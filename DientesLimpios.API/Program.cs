using DientesLimpios.API.Jobs;
using DientesLimpios.API.Middlewares;
using DientesLimpios.Aplicacion;
using DientesLimpios.Infraestructura;
using DientesLimpios.Persistencia;
using DientesLimpios.Identidad;
using DientesLimpios.Identidad.Modelos;
using Microsoft.AspNetCore.Mvc.Authorization;

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
