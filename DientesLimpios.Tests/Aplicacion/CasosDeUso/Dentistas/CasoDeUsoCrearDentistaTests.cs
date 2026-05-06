using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.CrearDentista;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.Errores;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DientesLimpios.Tests.Aplicacion.CasosDeUso.Dentistas
{
    public class CasoDeUsoCrearDentistaTests
    {
        private readonly IRepositorioDentistas _repositorio;
        private readonly IUnitOfwork _unidadDeTrabajo;
        private readonly HandlerCrearDentista _handler;
        private readonly ValidadorComandoCrearDentista _validator;
        private readonly ILogger<HandlerCrearDentista> _logger;

        public CasoDeUsoCrearDentistaTests()
        {
            // Aquí deberías inicializar los mocks o stubs necesarios para las pruebas
            // Por ejemplo, podrías usar Moq para crear un mock de IRepositorioPacientes
            // y IUnitOfwork, y luego pasarlos al handler y al validador.

            _repositorio = Substitute.For<IRepositorioDentistas>();
            _unidadDeTrabajo = Substitute.For<IUnitOfwork>();
            _logger = Substitute.For<ILogger<HandlerCrearDentista>>();
            _validator = new ValidadorComandoCrearDentista();

            _handler = new HandlerCrearDentista(_repositorio, _unidadDeTrabajo, _logger);
        }

        // Primero hacemos las pruebas propias del Handler:

        [Fact]
        public async Task Handle_DatosValidos_CreaDentistaPersisteYRetornaId()
        {
            // Arrange
            var comando = new ComandoCrearDentista { Nombre = "Felipe", Email = "felipe@ejemplo.com" };

            // Capture whatever Dentista the handler passes to Agregar.
            Dentista? dentistaCreadoEnHandler = null;

            _repositorio.Agregar(Arg.Do<Dentista>(d => dentistaCreadoEnHandler = d))
                        .Returns(c => c.Arg<Dentista>());   // echo back what was passed

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            dentistaCreadoEnHandler.Should().NotBeNull();
            dentistaCreadoEnHandler!.Nombre.Should().Be("Felipe");
            dentistaCreadoEnHandler.Email.Valor.Should().Be("felipe@ejemplo.com");
            result.Value.Should().Be(dentistaCreadoEnHandler.Id);   // ← compare against the captured Dentista
            await _repositorio.Received(1).Agregar(Arg.Any<Dentista>());
            await _unidadDeTrabajo.Received(1).Persistir();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Handle_NombreInvalido_RetornaFailureYNoPersiste(string? nombre)
        {
            // Arrange
            var comando = new ComandoCrearDentista { Nombre = nombre!, Email = "felipe@ejemplo.com" };

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Dentista.NombreObligatorio);
            await _repositorio.DidNotReceive().Agregar(Arg.Any<Dentista>());
            await _unidadDeTrabajo.DidNotReceive().Persistir();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Handle_EmailVacio_RetornaFailureEmailVacio(string? email)
        {
            // Arrange
            var comando = new ComandoCrearDentista { Nombre = "Felipe", Email = email! };

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.Vacio);
            await _repositorio.DidNotReceive().Agregar(Arg.Any<Dentista>());
            await _unidadDeTrabajo.DidNotReceive().Persistir();
        }

        [Theory]
        [InlineData("EmailInvalido")]
        [InlineData("sin-arroba.com")]
        public async Task Handle_EmailFormatoInvalido_RetornaFailureFormatoInvalido(string? email)
        {
            // Arrange
            var comando = new ComandoCrearDentista { Nombre = "Felipe", Email = email! };

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.FormatoInvalido);
            await _repositorio.DidNotReceive().Agregar(Arg.Any<Dentista>());
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
            var comando = new ComandoCrearDentista { Nombre = nombre!, Email = "felipe@ejemplo.com" };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Nombre);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("EmailInvalido")]      // no @
        [InlineData("sin-arroba.com")]     // no @
        public void Validar_EmailInvalido_GeneraErrorDeValidacion(string? email)
        {
            // Arrange
            var comando = new ComandoCrearDentista { Nombre = "Felipe", Email = email! };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Email);
        }

        [Fact]
        public void Validar_NombreYEmailValidos_NoGeneraErrorDeValidacion()
        {
            // Arrange
            var comando = new ComandoCrearDentista { Nombre = "Felipe", Email = "felipe@ejemplo.com" };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Nombre);
            result.ShouldNotHaveValidationErrorFor(c => c.Email);
        }
    }
}
