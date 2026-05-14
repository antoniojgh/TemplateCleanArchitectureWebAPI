using DientesLimpios.Application.UseCases.Offices.Commands.CreateOffice;
using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
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
        private readonly IOfficeRepository _repositorio;
        private readonly CreateOfficeCommandValidator _validator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateOfficeHandler> _logger;
        private readonly CreateOfficeHandler _handler;

        public CreateOfficeUseCaseTests()
        {
            _repositorio = Substitute.For<IOfficeRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _logger = Substitute.For<ILogger<CreateOfficeHandler>>();
            _validator = new CreateOfficeCommandValidator();

            _handler = new CreateOfficeHandler(_repositorio, _unitOfWork, _logger);
        }

        // First we write the Handler-specific tests:

        [Fact]
        public async Task Handle_CommandValido_CreaOfficeYRetornaSuId()
        {
            // Arrange
            var command = new CreateOfficeCommand { Name = "Office A" };

            // Capture whatever Office the handler passes to Add.
            Office? consultorioCreado = null;
            _repositorio.Add(Arg.Do<Office>(c => consultorioCreado = c))
                        .Returns(c => c.Arg<Office>());   // echo back what was passed

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            consultorioCreado.Should().NotBeNull();
            consultorioCreado!.Name.Should().Be("Office A");
            result.Value.Should().Be(consultorioCreado.Id);
            await _unitOfWork.Received(1).SaveChanges();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Handle_NombreInvalido_RetornaFailureYNoPersiste(string? name)
        {
            // Arrange
            var command = new CreateOfficeCommand { Name = name! };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Office.NameRequired);
            await _repositorio.DidNotReceive().Add(Arg.Any<Office>());
            await _unitOfWork.DidNotReceive().SaveChanges();
        }


        // Then we write the Validator-specific tests, since the validator
        // is a component external to the handler; validation is no longer done inside the Handler
        // but through an external validator injected via the "ValidationBehavior" class

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validar_NombreInvalido_GeneraErrorDeValidacion(string? name)
        {
            // Arrange
            var command = new CreateOfficeCommand { Name = name! };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name);
        }

        [Fact]
        public void Validator_NombreValido_NoGeneraErrorDeValidacion()
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
