using System.Diagnostics;
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

            // Anything reaching this handler is a bug or an infrastructure failure: expected
            // business outcomes travel as Result/Error and never throw.
            var problem = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = environment.IsDevelopment() ? exception.ToString() : OpaqueServerErrorDetail,
                Instance = httpContext.Request.Path,
                Type = "https://httpstatuses.io/500",
            };

            problem.Extensions["traceId"] = traceId;

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }
    }
}