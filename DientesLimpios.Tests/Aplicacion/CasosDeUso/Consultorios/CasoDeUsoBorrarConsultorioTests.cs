using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.BorrarConsultorio;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.Errores;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace DientesLimpios.Tests.Aplicacion.CasosDeUso.Consultorios
{
    public class CasoDeUsoBorrarConsultorioTests
    {
        private readonly IRepositorioConsultorios _repositorio;
        private readonly IUnitOfwork _unidadDeTrabajo;
        private readonly HandlerBorrarConsultorio _handler;
        private readonly ValidadorComandoBorrarConsultorio _validator;
        private readonly ILogger<HandlerBorrarConsultorio> _logger;

        public CasoDeUsoBorrarConsultorioTests()
        {
            _repositorio = Substitute.For<IRepositorioConsultorios>();
            _unidadDeTrabajo = Substitute.For<IUnitOfwork>();
            _logger = Substitute.For<ILogger<HandlerBorrarConsultorio>>();
            _validator = new ValidadorComandoBorrarConsultorio();

            _handler = new HandlerBorrarConsultorio(_repositorio, _unidadDeTrabajo, _logger);
        }

        // Primero hacemos las pruebas propias del Handler:

        [Fact]
        public async Task Handle_CuandoConsultorioExiste_BorraConsultorioYPersiste()
        {
            // Arrange
            var id = Guid.NewGuid();
            var comando = new ComandoBorrarConsultorio { Id = id };

            var consultorioResult = Consultorio.Crear("Consultorio A");
            var consultorio = consultorioResult.Value;

            _repositorio.ObtenerPorId(id).Returns(consultorio);

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            await _repositorio.Received(1).Borrar(consultorio);
            await _unidadDeTrabajo.Received(1).Persistir();
        }

        [Fact]
        public async Task Handle_CuandoConsultorioNoExiste_RetornaFailureNoEncontrado()
        {
            // Arrange
            var comando = new ComandoBorrarConsultorio { Id = Guid.NewGuid() };
            _repositorio.ObtenerPorId(comando.Id).ReturnsNull();

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Consultorio.NoEncontrado);
            await _repositorio.DidNotReceive().Borrar(Arg.Any<Consultorio>());
            await _unidadDeTrabajo.DidNotReceive().Persistir();
        }


        // Luego hacemos las pruebas propias de la validacion, ya que el validador
        // es un componente externo al handler, ya no validamos dentro del Handler sino que lo hacemos medieante
        // un validador externo que se inyecta en el handler mediante la clase "ValidationBehavior"

        [Fact]
        public void Validar_IdVacio_GeneraErrorDeValidacion()
        {
            // Arrange
            var comando = new ComandoBorrarConsultorio { Id = Guid.Empty};

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Id);
        }

        [Fact]
        public void Validar_IdValido_NoGeneraErrorDeValidacion()
        {
            // Arrange
            var comando = new ComandoBorrarConsultorio { Id = Guid.NewGuid()};

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Id);
        }
    }
}
