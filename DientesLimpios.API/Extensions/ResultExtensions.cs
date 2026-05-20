using DientesLimpios.Domain.Common.ResultPattern;
using Microsoft.AspNetCore.Mvc;


namespace DientesLimpios.API.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult(this Result result, HttpContext httpContext)
        {
            if (result.IsSuccess)
                return new NoContentResult();   // 204 instead of 200 OK for void commands

            return BuildProblemResult(result.Error, httpContext);
        }

        public static IActionResult ToActionResult<T>(this Result<T> result, HttpContext httpContext)
        {
            if (result.IsSuccess)
                return new OkObjectResult(result.Value);

            return BuildProblemResult(result.Error, httpContext);
        }

        public static IActionResult ToCreatedResult<T>(this Result<T> result, HttpContext httpContext, 
                                                       Func<T, string> locationBuilder)
        {
            if (result.IsFailure)
                return BuildProblemResult(result.Error, httpContext);

            var location = locationBuilder(result.Value);
            return new CreatedResult(location, result.Value);
        }

        private static IActionResult BuildProblemResult(Error error, HttpContext httpContext)
        {
            var status = MapStatusCode(error);

            var problem = new ProblemDetails
            {
                Status = status,
                Title = TitleFor(status),
                Detail = error.Message,
                Instance = httpContext.Request.Path,
                Type = $"https://httpstatuses.io/{status}",
            };

            problem.Extensions["errorCode"] = error.Code;

            if (error is ValidationError validationError)
            {
                problem.Extensions["errors"] = validationError.Errors
                    .Select(e => new { code = e.Code, message = e.Message })
                    .ToArray();
            }

            return new ObjectResult(problem) { StatusCode = status };
        }

        private static int MapStatusCode(Error error)
        {
            if (error is ValidationError)
                return StatusCodes.Status400BadRequest;

            // Convention: any error code ending in "NotFound" maps to 404.
            if (error.Code.EndsWith("NotFound", StringComparison.Ordinal))
                return StatusCodes.Status404NotFound;

            if (error.Code.EndsWith("Conflict", StringComparison.Ordinal) ||
                error.Code.EndsWith("Overlapping", StringComparison.Ordinal))
                return StatusCodes.Status409Conflict;

            return StatusCodes.Status400BadRequest;
        }

        private static string TitleFor(int status) => status switch
        {
            StatusCodes.Status404NotFound => "Resource not found",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status400BadRequest => "Invalid request",
            _ => "Error",
        };
    }

}
