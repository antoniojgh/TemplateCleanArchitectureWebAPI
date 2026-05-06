using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.CrearConsultorio;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.Errores;
using FluentAssertions; // For Should()
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using NSubstitute;      // For Substitute.For and Received()

namespace DientesLimpios.Tests.Aplicacion.CasosDeUso.Consultorios
{
    public class CasoDeUsoCrearConsultorioTests
    {
        // Fields are private and readonly because they are set in the constructor
        // and never change during the test execution.
        private readonly IRepositorioConsultorios _repositorio;
        private readonly ValidadorComandoCrearConsultorio _validator;
        private readonly IUnitOfwork _unidadDeTrabajo;
        private readonly ILogger<HandlerCrearConsultorio> _logger;
        private readonly HandlerCrearConsultorio _handler;

        public CasoDeUsoCrearConsultorioTests()
        {
            _repositorio = Substitute.For<IRepositorioConsultorios>();
            _unidadDeTrabajo = Substitute.For<IUnitOfwork>();
            _logger = Substitute.For<ILogger<HandlerCrearConsultorio>>();
            _validator = new ValidadorComandoCrearConsultorio();

            _handler = new HandlerCrearConsultorio(_repositorio, _unidadDeTrabajo, _logger);
        }

        // Primero hacemos las pruebas propias del Handler:

        [Fact]
        public async Task Handle_ComandoValido_CreaConsultorioYRetornaSuId()
        {
            // Arrange
            var comando = new ComandoCrearConsultorio { Nombre = "Consultorio A" };

            // Capture whatever Consultorio the handler passes to Agregar.
            Consultorio? consultorioCreado = null;
            _repositorio.Agregar(Arg.Do<Consultorio>(c => consultorioCreado = c))
                        .Returns(c => c.Arg<Consultorio>());   // echo back what was passed

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            consultorioCreado.Should().NotBeNull();
            consultorioCreado!.Nombre.Should().Be("Consultorio A");
            result.Value.Should().Be(consultorioCreado.Id);
            await _unidadDeTrabajo.Received(1).Persistir();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Handle_NombreInvalido_RetornaFailureYNoPersiste(string? nombre)
        {
            // Arrange
            var comando = new ComandoCrearConsultorio { Nombre = nombre! };

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Consultorio.NombreObligatorio);
            await _repositorio.DidNotReceive().Agregar(Arg.Any<Consultorio>());
            await _unidadDeTrabajo.DidNotReceive().Persistir();
        }


        // Luego hacemos las pruebas propias de la validacion, ya que el validador
        // es un componente externo al handler, ya no validamos dentro del Handler sino que lo hacemos medieante
        // un validador externo que se inyecta en el handler mediante la clase "ValidationBehavior"

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validar_NombreInvalido_GeneraErrorDeValidacion(string? nombre)
        {
            // Arrange
            var comando = new ComandoCrearConsultorio { Nombre = nombre! };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Nombre);
        }

        [Fact]
        public void Validador_NombreValido_NoGeneraErrorDeValidacion()
        {
            // Arrange
            var comando = new ComandoCrearConsultorio { Nombre = "Consultorio Central" };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Nombre);
        }
    }
}
