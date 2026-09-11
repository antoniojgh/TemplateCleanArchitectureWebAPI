using System.Text.Json;
using DientesLimpios.API.ExceptionHandlers;
using DientesLimpios.Application.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DientesLimpios.Tests.API
{
    public class GlobalExceptionHandlerTests
    {
        // Raw text of the kind SQL Server produces on a foreign-key violation. It names a
        // constraint and a table, so it must never reach a client outside Development.
        private const string SensitiveMessage =
            "The INSERT statement conflicted with the FOREIGN KEY constraint \"FK_Appointments_Patients_PatientId\".";

        [Fact]
        public async Task TryHandleAsync_UnknownExceptionInProduction_DoesNotEchoExceptionMessage()
        {
            // Arrange
            var handler = CreateHandler(Environments.Production);
            var context = CreateContext();

            // Act
            var handled = await handler.TryHandleAsync(context, new InvalidOperationException(SensitiveMessage), CancellationToken.None);

            // Assert
            handled.Should().BeTrue();
            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

            var problem = await ReadProblemAsync(context);
            problem.GetProperty("detail").GetString().Should().NotContain("FOREIGN KEY");
            problem.GetProperty("detail").GetString().Should().NotContain("FK_Appointments_Patients_PatientId");
        }

        [Fact]
        public async Task TryHandleAsync_UnknownExceptionInDevelopment_EchoesExceptionMessage()
        {
            // Arrange
            var handler = CreateHandler(Environments.Development);
            var context = CreateContext();

            // Act
            await handler.TryHandleAsync(context, new InvalidOperationException(SensitiveMessage), CancellationToken.None);

            // Assert
            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

            var problem = await ReadProblemAsync(context);
            problem.GetProperty("detail").GetString().Should().Be(SensitiveMessage);
        }

        [Fact]
        public async Task TryHandleAsync_MediatorExceptionInProduction_DoesNotEchoExceptionMessage()
        {
            // Arrange
            var handler = CreateHandler(Environments.Production);
            var context = CreateContext();

            // Act
            await handler.TryHandleAsync(context, new MediatorException("No handler registered for CreateAppointmentCommand"), CancellationToken.None);

            // Assert
            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

            var problem = await ReadProblemAsync(context);
            problem.GetProperty("title").GetString().Should().Be("Dispatch error");
            problem.GetProperty("detail").GetString().Should().NotContain("CreateAppointmentCommand");
        }

        [Fact]
        public async Task TryHandleAsync_NotFoundExceptionInProduction_EchoesExceptionMessage()
        {
            // Arrange
            var handler = CreateHandler(Environments.Production);
            var context = CreateContext();
            var exception = new NotFoundException("Patient", Guid.Empty);

            // Act
            await handler.TryHandleAsync(context, exception, CancellationToken.None);

            // Assert
            context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);

            var problem = await ReadProblemAsync(context);
            problem.GetProperty("detail").GetString().Should().Be(exception.Message);
        }

        [Fact]
        public async Task TryHandleAsync_ValidationExceptionInProduction_EchoesExceptionMessage()
        {
            // Arrange
            var handler = CreateHandler(Environments.Production);
            var context = CreateContext();

            // Act
            await handler.TryHandleAsync(context, new ValidationException("Name is required"), CancellationToken.None);

            // Assert
            context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

            var problem = await ReadProblemAsync(context);
            problem.GetProperty("detail").GetString().Should().Be("Name is required");
        }

        [Fact]
        public async Task TryHandleAsync_UnknownExceptionInProduction_WritesTraceIdForCorrelation()
        {
            // Arrange
            var handler = CreateHandler(Environments.Production);
            var context = CreateContext();

            // Act
            await handler.TryHandleAsync(context, new InvalidOperationException(SensitiveMessage), CancellationToken.None);

            // Assert
            var problem = await ReadProblemAsync(context);
            problem.GetProperty("traceId").GetString().Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task TryHandleAsync_ResponseAlreadyStarted_ReturnsFalseWithoutWriting()
        {
            // Arrange
            var handler = CreateHandler(Environments.Production);
            var body = new MemoryStream();
            var features = new FeatureCollection();
            features.Set<IHttpRequestFeature>(new HttpRequestFeature { Path = "/api/v1/appointments", Method = "POST" });
            features.Set<IHttpResponseFeature>(new StartedResponseFeature());
            features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(body));
            var context = new DefaultHttpContext(features);

            // Act
            var handled = await handler.TryHandleAsync(context, new InvalidOperationException(SensitiveMessage), CancellationToken.None);

            // Assert
            handled.Should().BeFalse();
            body.Length.Should().Be(0);
        }

        private static GlobalExceptionHandler CreateHandler(string environmentName)
        {
            var environment = Substitute.For<IHostEnvironment>();
            environment.EnvironmentName.Returns(environmentName);

            return new GlobalExceptionHandler(Substitute.For<ILogger<GlobalExceptionHandler>>(), environment);
        }

        private static DefaultHttpContext CreateContext()
        {
            var context = new DefaultHttpContext { Request = { Path = "/api/v1/appointments", Method = "POST" } };
            context.Response.Body = new MemoryStream();

            return context;
        }

        private static async Task<JsonElement> ReadProblemAsync(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var document = await JsonDocument.ParseAsync(context.Response.Body);

            return document.RootElement.Clone();
        }

        // DefaultHttpContext's response feature always reports HasStarted = false;
        // this override is the only way to exercise the already-started branch.
        private sealed class StartedResponseFeature : HttpResponseFeature
        {
            public override bool HasStarted => true;
        }
    }
}
