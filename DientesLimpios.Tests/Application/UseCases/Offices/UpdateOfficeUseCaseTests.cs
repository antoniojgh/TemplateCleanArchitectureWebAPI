using DientesLimpios.Application.UseCases.Offices.Commands.UpdateOffice;
using DientesLimpios.Application.UseCases.Offices.Commands.DeleteOffice;
using DientesLimpios.Application.Exceptions;
using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace DientesLimpios.Tests.Application.UseCases.Offices
{
    public class UpdateOfficeUseCaseTests
    {
        private readonly IOfficeRepository _repositorio;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UpdateOfficeHandler _handler;
        private readonly UpdateOfficeCommandValidator _validator;
        private readonly ILogger<UpdateOfficeHandler> _logger;


        public UpdateOfficeUseCaseTests()
        {
            _repositorio = Substitute.For<IOfficeRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _logger = Substitute.For<ILogger<UpdateOfficeHandler>>();
            _validator = new UpdateOfficeCommandValidator();

            _handler = new UpdateOfficeHandler(_repositorio, _unitOfWork, _logger);
        }

        // Primero hacemos las pruebas propias del Handler:

        [Fact]
        public async Task Handle_CuandoOfficeExiste_ActualizaNombreYPersiste()
        {
            // Arrange
            var officeResult = Office.Create("Office A");
            var office = officeResult.Value;

            var id = office.Id;
            var command = new UpdateOfficeCommand { Id = id, Name = "Nuevo name" };

            _repositorio.GetById(id).Returns(office);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            await _repositorio.Received(1).Update(office);
            await _unitOfWork.Received(1).SaveChanges();

        }

        [Fact]
        public async Task Handle_CuandoOfficeNoExiste_RetornaFailureNotFound()
        {
            // Arrange
            var command = new UpdateOfficeCommand { Id = Guid.NewGuid(), Name = "Name" };
            _repositorio.GetById(command.Id).ReturnsNull();

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Office.NotFound);
            await _repositorio.DidNotReceive().Update(Arg.Any<Office>());
            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        // Luego hacemos las pruebas propias de la validacion, ya que el validator
        // es un componente externo al handler, ya no validamos dentro del Handler sino que lo hacemos medieante
        // un validator externo que se inyecta en el handler mediante la clase "ValidationBehavior"

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Validator_NombreVacio_GeneraErrorDeValidacion(string? name)
        {
            // Arrange
            var command = new UpdateOfficeCommand { Id = Guid.NewGuid(), Name = name! };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Name);
        }

        [Fact]
        public void Validator_NombreValido_NoGeneraErrorDeValidacion()
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
