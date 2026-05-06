using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.ActualizarConsultorio;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.BorrarConsultorio;
using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Comunes.PatronResultados;
using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.Errores;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace DientesLimpios.Tests.Aplicacion.CasosDeUso.Consultorios
{
    public class CasoDeUsoActualizarConsultorioTests
    {
        private readonly IRepositorioConsultorios _repositorio;
        private readonly IUnitOfwork _unidadDeTrabajo;
        private readonly HandlerActualizarConsultorio _handler;
        private readonly ValidadorComandoActualizarConsultorio _validator;
        private readonly ILogger<HandlerActualizarConsultorio> _logger;


        public CasoDeUsoActualizarConsultorioTests()
        {
            _repositorio = Substitute.For<IRepositorioConsultorios>();
            _unidadDeTrabajo = Substitute.For<IUnitOfwork>();
            _logger = Substitute.For<ILogger<HandlerActualizarConsultorio>>();
            _validator = new ValidadorComandoActualizarConsultorio();

            _handler = new HandlerActualizarConsultorio(_repositorio, _unidadDeTrabajo, _logger);
        }

        // Primero hacemos las pruebas propias del Handler:

        [Fact]
        public async Task Handle_CuandoConsultorioExiste_ActualizaNombreYPersiste()
        {
            // Arrange
            var consultorioResult = Consultorio.Crear("Consultorio A");
            var consultorio = consultorioResult.Value;

            var id = consultorio.Id;
            var comando = new ComandoActualizarConsultorio { Id = id, Nombre = "Nuevo nombre" };

            _repositorio.ObtenerPorId(id).Returns(consultorio);

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            await _repositorio.Received(1).Actualizar(consultorio);
            await _unidadDeTrabajo.Received(1).Persistir();

        }

        [Fact]
        public async Task Handle_CuandoConsultorioNoExiste_RetornaFailureNoEncontrado()
        {
            // Arrange
            var comando = new ComandoActualizarConsultorio { Id = Guid.NewGuid(), Nombre = "Nombre" };
            _repositorio.ObtenerPorId(comando.Id).ReturnsNull();

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Consultorio.NoEncontrado);
            await _repositorio.DidNotReceive().Actualizar(Arg.Any<Consultorio>());
            await _unidadDeTrabajo.DidNotReceive().Persistir();
        }

        // Luego hacemos las pruebas propias de la validacion, ya que el validador
        // es un componente externo al handler, ya no validamos dentro del Handler sino que lo hacemos medieante
        // un validador externo que se inyecta en el handler mediante la clase "ValidationBehavior"

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Validador_NombreVacio_GeneraErrorDeValidacion(string? nombre)
        {
            // Arrange
            var comando = new ComandoActualizarConsultorio { Id = Guid.NewGuid(), Nombre = nombre! };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Nombre);
        }

        [Fact]
        public void Validador_NombreValido_NoGeneraErrorDeValidacion()
        {
            // Arrange
            var comando = new ComandoActualizarConsultorio { Id = Guid.NewGuid(), Nombre = "Consultorio Central" };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Nombre);
        }

    }
}
