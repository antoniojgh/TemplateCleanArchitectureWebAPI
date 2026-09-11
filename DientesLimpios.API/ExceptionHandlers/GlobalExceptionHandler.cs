using System.Diagnostics;
using DientesLimpios.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DientesLimpios.API.ExceptionHandlers
{
    public sealed class GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
        : IExceptionHandler
    {
        // Returned instead of the exception message for 5xx outside Development.
        // Exception text can carry SQL constraint names, table names and connection
        // details; none of that may reach a client.
        private const string OpaqueServerErrorDetail =
            "An unexpected error occurred while processing the request. " +
            "Quote the traceId when reporting this problem.";

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            logger.LogError(exception,
                "Unhandled exception in {Method} {Path} with {TraceId}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                traceId);

            // Once the response has started, status and headers are locked in and writing
            // a body here would corrupt the payload. Let the server tear the connection down.
            if (httpContext.Response.HasStarted)
                return false;

            // Safety net: these exception types are not thrown by current Domain/Application
            // code (everything uses Result now), but the handler retains the mapping for
            // future code paths that may still rely on exceptions for control flow.

            var (status, title) = exception switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
                ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
                MediatorException => (StatusCodes.Status500InternalServerError, "Dispatch error"),
                _ => (StatusCodes.Status500InternalServerError, "Internal server error"),
            };

            // 4xx messages originate in our own exception types and are safe to return.
            // 5xx messages originate anywhere, so they are only exposed in Development.
            var exposeMessage = status < StatusCodes.Status500InternalServerError
                                || environment.IsDevelopment();

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = exposeMessage ? exception.Message : OpaqueServerErrorDetail,
                Instance = httpContext.Request.Path,
                Type = $"https://httpstatuses.io/{status}",
            };

            problem.Extensions["traceId"] = traceId;

            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }
    }
}