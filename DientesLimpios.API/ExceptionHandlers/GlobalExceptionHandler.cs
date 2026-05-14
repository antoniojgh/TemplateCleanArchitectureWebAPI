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
                "Unhandled exception in {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            var (status, title) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
                ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
                BusinessRuleException => (StatusCodes.Status400BadRequest, "Business rule violated"),
                MediatorException => (StatusCodes.Status500InternalServerError, "Dispatch error"),
                _ => (StatusCodes.Status500InternalServerError, "Internal server error"),
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
