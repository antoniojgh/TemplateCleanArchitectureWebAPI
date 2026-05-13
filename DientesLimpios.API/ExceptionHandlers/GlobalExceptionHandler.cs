using DientesLimpios.Application.Exceptions;
using DientesLimpios.Domain.Exceptions;
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
                NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
                ValidationException => (StatusCodes.Status400BadRequest, "Validación fallida"),
                BusinessRuleException => (StatusCodes.Status400BadRequest, "Regla de negocio violada"),
                MediatorException => (StatusCodes.Status500InternalServerError, "Error de despacho"),
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
