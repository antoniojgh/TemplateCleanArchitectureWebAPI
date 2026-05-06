using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Dominio.Excepciones;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DientesLimpios.API.ExceptionHandlers
{
    public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            logger.LogError(exception,
                "Excepción no controlada en {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            var (status, title) = exception switch
            {
                ExcepcionNoEncontrado => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
                ExcepcionDeValidacion => (StatusCodes.Status400BadRequest, "Validación fallida"),
                ExcepcionDeReglaDeNegocio => (StatusCodes.Status400BadRequest, "Regla de negocio violada"),
                ExcepcionDeMediador => (StatusCodes.Status500InternalServerError, "Error de despacho"),
                _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor"),
            };

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = exception.Message,
                Instance = httpContext.Request.Path,
                Type = $"https://httpstatuses.io/{status}",
            };

            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }
    }

}
