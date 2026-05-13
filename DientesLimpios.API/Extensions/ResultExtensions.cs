using DientesLimpios.Application.Utilities.ResultPattern;
using DientesLimpios.Domain.Common.ResultPattern;
using Microsoft.AspNetCore.Mvc;


namespace DientesLimpios.API.Extensions
{
    public static class ResultExtensions
    {
        public static IActionResult ToActionResult(this Result result, HttpContext httpContext)
        {
            if (result.IsSuccess)
                return new OkResult();

            return BuildProblemResult(result.Error, httpContext);
        }

        public static IActionResult ToActionResult<T>(this Result<T> result, HttpContext httpContext)
        {
            if (result.IsSuccess)
                return new OkObjectResult(result.Value);

            return BuildProblemResult(result.Error, httpContext);
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
            // Convention: codes ending in NotFound/NotFound → 404.
            if (error.Code.EndsWith("NotFound", StringComparison.Ordinal) ||
                error.Code.EndsWith("NotFound", StringComparison.Ordinal))
                return StatusCodes.Status404NotFound;
            else
                return StatusCodes.Status400BadRequest;
        }

        private static string TitleFor(int status) => status switch
        {
            StatusCodes.Status404NotFound => "Recurso no encontrado",
            StatusCodes.Status400BadRequest => "Solicitud inválida",
            _ => "Error",
        };
    }

}
