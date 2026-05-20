using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.UseCases.Offices.Commands.CreateOffice;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using FluentAssertions; // For Should()
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using NSubstitute;      // For Substitute.For and Received()

namespace DientesLimpios.Tests.Application.UseCases.Offices
{
    public class CreateOfficeUseCaseTests
    {
        // Fields are private and readonly because they are set in the constructor
        // and never change during the test execution.
        private readonly IApplicationDbContext _db;
        private readonly CreateOfficeCommandValidator _validator;
        private readonly ILogger<CreateOfficeHandler> _logger;
        private readonly CreateOfficeHandler _handler;

        public CreateOfficeUseCaseTests()
        {
            _db = Substitute.For<IApplicationDbContext>();
            _logger = Substitute.For<ILogger<CreateOfficeHandler>>();
            _validator = new CreateOfficeCommandValidator();

            _handler = new CreateOfficeHandler(_db, _logger);
        }

        // First we write the Handler-specific tests:

        [Fact]
        public async Task Handle_ValidCommand_CreatesOfficeAndReturnsId()
        {
            // Arrange
            var command = new CreateOfficeCommand { Name = "Office A" };

            // Capture whatever Office the handler passes to Add.
            Office? createdOffice = null;
            _db.Offices.When(s => s.Add(Arg.Any<Office>()))
                       .Do(call => createdOffice = call.Arg<Office>());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            createdOffice.Should().NotBeNull();
            createdOffice!.Name.Should().Be("Office A");
            result.Value.Should().Be(createdOffice.Id);
            _db.Offices.Received(1).Add(Arg.Any<Office>());
            await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Handle_InvalidName_ReturnsFailureAndDoesNotPersist(string? name)
        {
            // Arrange
            var command = new CreateOfficeCommand { Name = name! };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Office.NameRequired);
            _db.Offices.DidNotReceive().Add(Arg.Any<Office>());
            await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }


        // Then we write the Validator-specific tests, since the validator
        // is a component external to the handler; validation is no longer done inside the Handler
        // but through an external validator injected via the "ValidationBehavior" class

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_InvalidName_GeneratesValidationError(string? name)
        {
            // Arrange
            var command = new CreateOfficeCommand { Name = name! };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name);
        }

        [Fact]
        public void Validate_ValidName_GeneratesNoValidationError()
        {
            // Arrange
            var command = new CreateOfficeCommand { Name = "Office Central" };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Name);
        }
    }
}
