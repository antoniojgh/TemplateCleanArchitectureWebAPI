using DientesLimpios.API.Extensions;
using DientesLimpios.Domain.Common.ResultPattern;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace DientesLimpios.Tests.API
{
    public class ResultExtensionsTests
    {
        private readonly HttpContext _ctx = new DefaultHttpContext { Request = { Path = "/api/v1/test" } };

        [Fact]
        public void Success_NonGeneric_Returns_OkResult()
        {
            var result = Result.Success();
            var action = result.ToActionResult(_ctx);
            action.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public void Success_Generic_Returns_OkObjectResult_WithValue()
        {
            var result = Result.Success(42);
            var action = result.ToActionResult(_ctx);
            action.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(42);
        }

        [Fact]
        public void Failure_NotFound_Returns_404_ProblemDetails()
        {
            var error = new Error("Patient.NotFound", "Patient not found");
            var result = Result.Failure(error);
            var action = result.ToActionResult(_ctx) as ObjectResult;

            action.Should().NotBeNull();
            action!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            var problem = action.Value.Should().BeOfType<ProblemDetails>().Subject;
            problem.Status.Should().Be(404);
            problem.Extensions["errorCode"].Should().Be("Patient.NotFound");
        }

        [Fact]
        public void Failure_Generic_Error_Returns_400()
        {
            var error = new Error("Appointment.InThePast", "...");
            var result = Result.Failure<Guid>(error);
            var action = result.ToActionResult(_ctx) as ObjectResult;
            action!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        [Fact]
        public void Failure_ValidationError_Returns_400_WithErrorsArray()
        {
            var fields = new[]
            {
            new Error("Name", "Name is required"),
            new Error("Email", "Email is invalid"),
        };
            var validation = new ValidationError(fields);
            var result = Result.Failure(validation);
            var action = result.ToActionResult(_ctx) as ObjectResult;
            action!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            var problem = action.Value.Should().BeOfType<ProblemDetails>().Subject;
            problem.Extensions.Should().ContainKey("errors");
        }



    }
}
