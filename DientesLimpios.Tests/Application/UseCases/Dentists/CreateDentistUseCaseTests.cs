using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.UseCases.Dentists.Commands.CreateDentist;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Dentists
{
    public class CreateDentistUseCaseTests
    {
        private readonly IApplicationDbContext _db;
        private readonly CreateDentistHandler _handler;
        private readonly CreateDentistCommandValidator _validator;
        private readonly ILogger<CreateDentistHandler> _logger;

        public CreateDentistUseCaseTests()
        {
            // Here you should initialize the mocks or stubs needed for the tests

            _db = Substitute.For<IApplicationDbContext>();
            _logger = Substitute.For<ILogger<CreateDentistHandler>>();
            _validator = new CreateDentistCommandValidator();

            _handler = new CreateDentistHandler(_db, _logger);
        }

        // First we write the Handler-specific tests:

        [Fact]
        public async Task Handle_ValidData_CreatesDentistPersistsAndReturnsId()
        {
            // Arrange
            var command = new CreateDentistCommand { Name = "Felipe", Email = "felipe@ejemplo.com" };

            // Capture whatever Dentist the handler passes to Add.
            Dentist? createdDentist = null;
            _db.Dentists.When(s => s.Add(Arg.Any<Dentist>()))
                        .Do(call => createdDentist = call.Arg<Dentist>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            createdDentist.Should().NotBeNull();
            createdDentist!.Name.Should().Be("Felipe");
            createdDentist.Email.Value.Should().Be("felipe@ejemplo.com");
            result.Value.Should().Be(createdDentist.Id);   // ← compare against the captured Dentist
            _db.Dentists.Received(1).Add(Arg.Any<Dentist>());
            await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Handle_InvalidName_ReturnsFailureAndDoesNotPersist(string? name)
        {
            // Arrange
            var command = new CreateDentistCommand { Name = name!, Email = "felipe@ejemplo.com" };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Dentist.NameRequired);
            _db.Dentists.DidNotReceive().Add(Arg.Any<Dentist>());
            await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Handle_EmptyEmail_ReturnsFailureEmailEmpty(string? email)
        {
            // Arrange
            var command = new CreateDentistCommand { Name = "Felipe", Email = email! };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.Empty);
            _db.Dentists.DidNotReceive().Add(Arg.Any<Dentist>());
            await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData("EmailInvalido")]
        [InlineData("sin-arroba.com")]
        public async Task Handle_InvalidEmailFormat_ReturnsFailureInvalidFormat(string? email)
        {
            // Arrange
            var command = new CreateDentistCommand { Name = "Felipe", Email = email! };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.InvalidFormat);
            _db.Dentists.DidNotReceive().Add(Arg.Any<Dentist>());
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
            var command = new CreateDentistCommand { Name = name!, Email = "felipe@ejemplo.com" };

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
            var command = new CreateDentistCommand { Name = "Felipe", Email = email! };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Email);
        }

        [Fact]
        public void Validate_ValidNameAndEmail_GeneratesNoValidationError()
        {
            // Arrange
            var command = new CreateDentistCommand { Name = "Felipe", Email = "felipe@ejemplo.com" };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Name);
            result.ShouldNotHaveValidationErrorFor(c => c.Email);
        }
    }
}
