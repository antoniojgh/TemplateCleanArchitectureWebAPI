using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.UseCases.Offices.Commands.DeleteOffice;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Errors;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace DientesLimpios.Tests.Application.UseCases.Offices
{
    public class DeleteOfficeUseCaseTests
    {
        private readonly IApplicationDbContext _db;
        private readonly DeleteOfficeHandler _handler;
        private readonly DeleteOfficeCommandValidator _validator;
        private readonly ILogger<DeleteOfficeHandler> _logger;

        public DeleteOfficeUseCaseTests()
        {
            _db = Substitute.For<IApplicationDbContext>();
            _logger = Substitute.For<ILogger<DeleteOfficeHandler>>();
            _validator = new DeleteOfficeCommandValidator();

            _handler = new DeleteOfficeHandler(_db, _logger);
        }

        // First we write the Handler-specific tests:

        [Fact]
        public async Task Handle_CuandoOfficeExiste_BorraOfficeYPersiste()
        {
            // Arrange
            var officeResult = Office.Create("Office A");
            var office = officeResult.Value;

            var id = office.Id;
            var command = new DeleteOfficeCommand { Id = id };

            var dbSet = new List<Office> { office }.BuildMockDbSet();
            _db.Offices.Returns(dbSet);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            dbSet.Received(1).Remove(office);
            await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_CuandoOfficeNoExiste_RetornaFailureNotFound()
        {
            // Arrange
            var command = new DeleteOfficeCommand { Id = Guid.NewGuid() };

            var dbSet = new List<Office>().BuildMockDbSet();
            _db.Offices.Returns(dbSet);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Office.NotFound);
            _db.Offices.DidNotReceive().Remove(Arg.Any<Office>());
            await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
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
