using DientesLimpios.Application.UseCases.Offices.Commands.DeleteOffice;
using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace DientesLimpios.Tests.Application.UseCases.Offices
{
    public class DeleteOfficeUseCaseTests
    {
        private readonly IOfficeRepository _repositorio;
        private readonly IUnitOfWork _unitOfWork;
        private readonly DeleteOfficeHandler _handler;
        private readonly DeleteOfficeCommandValidator _validator;
        private readonly ILogger<DeleteOfficeHandler> _logger;

        public DeleteOfficeUseCaseTests()
        {
            _repositorio = Substitute.For<IOfficeRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _logger = Substitute.For<ILogger<DeleteOfficeHandler>>();
            _validator = new DeleteOfficeCommandValidator();

            _handler = new DeleteOfficeHandler(_repositorio, _unitOfWork, _logger);
        }

        // First we write the Handler-specific tests:

        [Fact]
        public async Task Handle_CuandoOfficeExiste_BorraOfficeYPersiste()
        {
            // Arrange
            var id = Guid.NewGuid();
            var command = new DeleteOfficeCommand { Id = id };

            var officeResult = Office.Create("Office A");
            var office = officeResult.Value;

            _repositorio.GetById(id).Returns(office);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            await _repositorio.Received(1).Delete(office);
            await _unitOfWork.Received(1).SaveChanges();
        }

        [Fact]
        public async Task Handle_CuandoOfficeNoExiste_RetornaFailureNotFound()
        {
            // Arrange
            var command = new DeleteOfficeCommand { Id = Guid.NewGuid() };
            _repositorio.GetById(command.Id).ReturnsNull();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Office.NotFound);
            await _repositorio.DidNotReceive().Delete(Arg.Any<Office>());
            await _unitOfWork.DidNotReceive().SaveChanges();
        }


        // Luego hacemos las pruebas propias de la validacion, ya que el validator
        // es un componente externo al handler, ya no validamos dentro del Handler sino que lo hacemos medieante
        // un validator externo que se inyecta en el handler mediante la clase "ValidationBehavior"

        [Fact]
        public void Validar_IdVacio_GeneraErrorDeValidacion()
        {
            // Arrange
            var command = new DeleteOfficeCommand { Id = Guid.Empty};

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Id);
        }

        [Fact]
        public void Validar_IdValido_NoGeneraErrorDeValidacion()
        {
            // Arrange
            var command = new DeleteOfficeCommand { Id = Guid.NewGuid()};

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Id);
        }
    }
}
