using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.UseCases.Patients.Commands.CreatePatient;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Patients
{
    public class CreatePatientUseCaseTests
    {
        private readonly IApplicationDbContext _db;
        private readonly CreatePatientHandler _handler;
        private readonly CreatePatientCommandValidator _validator;
        private readonly ILogger<CreatePatientHandler> _logger;

        public CreatePatientUseCaseTests()
        {
            // Here you should initialize the mocks or stubs needed for the tests

            _db = Substitute.For<IApplicationDbContext>();
            _logger = Substitute.For<ILogger<CreatePatientHandler>>();
            _validator = new CreatePatientCommandValidator();

            _handler = new CreatePatientHandler(_db, _logger);
        }

        // First we write the Handler-specific tests:

        [Fact]
        public async Task Handle_CuandoDatosValidos_CreaPatientPersisteYRetornaId()
        {
            // Arrange
            var command = new CreatePatientCommand { Name = "Felipe", Email = "felipe@ejemplo.com" };

            // Capture whatever Patient the handler passes to Add.
            Patient? pacienteCreadoEnHandler = null;

            _db.Patients.When(s => s.Add(Arg.Any<Patient>()))
                        .Do(call => pacienteCreadoEnHandler = call.Arg<Patient>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            pacienteCreadoEnHandler.Should().NotBeNull();
            pacienteCreadoEnHandler!.Name.Should().Be("Felipe");
            pacienteCreadoEnHandler.Email.Value.Should().Be("felipe@ejemplo.com");
            result.Value.Should().Be(pacienteCreadoEnHandler.Id);   // ← compare against the captured Patient
            _db.Patients.Received(1).Add(Arg.Any<Patient>());
            await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Handle_NombreInvalido_RetornaFailureYNoPersiste(string? name)
        {
            // Arrange
            var command = new CreatePatientCommand { Name = name!, Email = "felipe@ejemplo.com" };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Patient.NameRequired);
            _db.Patients.DidNotReceive().Add(Arg.Any<Patient>());
            await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Handle_EmailVacio_RetornaFailureEmailVacio(string? email)
        {
            // Arrange
            var command = new CreatePatientCommand { Name = "Felipe", Email = email! };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.Empty);
            _db.Patients.DidNotReceive().Add(Arg.Any<Patient>());
            await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData("EmailInvalido")]
        [InlineData("sin-arroba.com")]
        public async Task Handle_EmailInvalidFormat_RetornaFailureInvalidFormat(string? email)
        {
            // Arrange
            var command = new CreatePatientCommand { Name = "Felipe", Email = email! };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.InvalidFormat);
            _db.Patients.DidNotReceive().Add(Arg.Any<Patient>());
            await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        // Luego hacemos las pruebas propias de la validacion, ya que el validator
        // es un componente externo al handler, ya no validamos dentro del Handler sino que lo hacemos medieante
        // un validator externo que se inyecta en el handler mediante la clase "ValidationBehavior"

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validar_NombreInvalido_GeneraErrorDeValidacion(string? name)
        {
            // Arrange
            var command = new CreatePatientCommand { Name = name!, Email = "felipe@ejemplo.com" };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("EmailInvalido")]      // no @
        [InlineData("sin-arroba.com")]     // no @
        public void Validar_EmailInvalido_GeneraErrorDeValidacion(string? email)
        {
            // Arrange
            var command = new CreatePatientCommand { Name = "Felipe", Email = email! };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Email);
        }

        [Fact]
        public void Validator_NombreYEmailValido_NoGeneraErrorDeValidacion()
        {
            // Arrange
            var command = new CreatePatientCommand { Name = "Felipe", Email = "felipe@ejemplo.com" };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Name);
            result.ShouldNotHaveValidationErrorFor(c => c.Email);
        }
    }
}
