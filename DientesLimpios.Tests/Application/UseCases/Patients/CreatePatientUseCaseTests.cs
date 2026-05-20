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
        public async Task Handle_ValidData_CreatesPatientPersistsAndReturnsId()
        {
            // Arrange
            var command = new CreatePatientCommand { Name = "Felipe", Email = "felipe@ejemplo.com" };

            // Capture whatever Patient the handler passes to Add.
            Patient? createdPatient = null;

            _db.Patients.When(s => s.Add(Arg.Any<Patient>()))
                        .Do(call => createdPatient = call.Arg<Patient>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            createdPatient.Should().NotBeNull();
            createdPatient!.Name.Should().Be("Felipe");
            createdPatient.Email.Value.Should().Be("felipe@ejemplo.com");
            result.Value.Should().Be(createdPatient.Id);   // ← compare against the captured Patient
            _db.Patients.Received(1).Add(Arg.Any<Patient>());
            await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Handle_InvalidName_ReturnsFailureAndDoesNotPersist(string? name)
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
        public async Task Handle_EmptyEmail_ReturnsFailureEmailEmpty(string? email)
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
        public async Task Handle_InvalidEmailFormat_ReturnsFailureInvalidFormat(string? email)
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

        // The validator is an external component; validation is no longer done inside the Handler
        // but through an external validator injected via the "ValidationBehavior" class

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_InvalidName_GeneratesValidationError(string? name)
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
        public void Validate_InvalidEmail_GeneratesValidationError(string? email)
        {
            // Arrange
            var command = new CreatePatientCommand { Name = "Felipe", Email = email! };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Email);
        }

        [Fact]
        public void Validate_ValidNameAndEmail_GeneratesNoValidationError()
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
