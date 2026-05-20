using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.UseCases.Offices.Commands.UpdateOffice;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Offices
{
    public class UpdateOfficeUseCaseTests
    {
        private readonly IApplicationDbContext _db;
        private readonly UpdateOfficeHandler _handler;
        private readonly UpdateOfficeCommandValidator _validator;
        private readonly ILogger<UpdateOfficeHandler> _logger;


        public UpdateOfficeUseCaseTests()
        {
            _db = Substitute.For<IApplicationDbContext>();
            _logger = Substitute.For<ILogger<UpdateOfficeHandler>>();
            _validator = new UpdateOfficeCommandValidator();

            _handler = new UpdateOfficeHandler(_db, _logger);
        }

        // First we write the Handler-specific tests:

        [Fact]
        public async Task Handle_OfficeExists_UpdatesNameAndPersists()
        {
            // Arrange
            var officeResult = Office.Create("Office A");
            var office = officeResult.Value;

            var id = office.Id;
            var command = new UpdateOfficeCommand { Id = id, Name = "Nuevo name" };

            var dbSet = new List<Office> { office }.BuildMockDbSet();
            _db.Offices.Returns(dbSet);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            var officeInDb = await dbSet.FirstAsync();
            officeInDb.Name.Should().Be("Nuevo name");

        }

        [Fact]
        public async Task Handle_OfficeNotFound_ReturnsFailureNotFound()
        {
            // Arrange
            var command = new UpdateOfficeCommand { Id = Guid.NewGuid(), Name = "Name" };

            var dbSet = new List<Office>().BuildMockDbSet();
            _db.Offices.Returns(dbSet);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Office.NotFound);
            await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        // Then we write the Validator-specific tests, since the validator
        // is a component external to the handler; validation is no longer done inside the Handler
        // but through an external validator injected via the "ValidationBehavior" class

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Validator_EmptyName_GeneratesValidationError(string? name)
        {
            // Arrange
            var command = new UpdateOfficeCommand { Id = Guid.NewGuid(), Name = name! };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name);
        }

        [Fact]
        public void Validator_ValidName_GeneratesNoValidationError()
        {
            // Arrange
            var command = new UpdateOfficeCommand { Id = Guid.NewGuid(), Name = "Office Central" };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Name);
        }

    }
}
