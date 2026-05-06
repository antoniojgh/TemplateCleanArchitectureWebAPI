using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.CrearDentista;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.CrearPaciente;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.Errores;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DientesLimpios.Tests.Aplicacion.CasosDeUso.Pacientes
{
    public class CasoDeUsoCrearPacienteTests
    {
        private readonly IRepositorioPacientes _repositorio;
        private readonly IUnitOfwork _unidadDeTrabajo;
        private readonly HandlerCrearPaciente _handler;
        private readonly ValidadorComandoCrearPaciente _validator;
        private readonly ILogger<HandlerCrearPaciente> _logger;

        public CasoDeUsoCrearPacienteTests()
        {
            // Aquí deberías inicializar los mocks o stubs necesarios para las pruebas
            // Por ejemplo, podrías usar Moq para crear un mock de IRepositorioPacientes
            // y IUnitOfwork, y luego pasarlos al handler y al validador.

            _repositorio = Substitute.For<IRepositorioPacientes>();
            _unidadDeTrabajo = Substitute.For<IUnitOfwork>();
            _logger = Substitute.For<ILogger<HandlerCrearPaciente>>();
            _validator = new ValidadorComandoCrearPaciente();

            _handler = new HandlerCrearPaciente(_repositorio, _unidadDeTrabajo, _logger);
        }

        // Primero hacemos las pruebas propias del Handler:

        [Fact]
        public async Task Handle_CuandoDatosValidos_CreaPacientePersisteYRetornaId()
        {
            // Arrange
            var comando = new ComandoCrearPaciente { Nombre = "Felipe", Email = "felipe@ejemplo.com" };

            // Capture whatever Paciente the handler passes to Agregar.
            Paciente? pacienteCreadoEnHandler = null;

            _repositorio.Agregar(Arg.Do<Paciente>(d => pacienteCreadoEnHandler = d))
                        .Returns(c => c.Arg<Paciente>());   // echo back what was passed

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            pacienteCreadoEnHandler.Should().NotBeNull();
            pacienteCreadoEnHandler!.Nombre.Should().Be("Felipe");
            pacienteCreadoEnHandler.Email.Valor.Should().Be("felipe@ejemplo.com");
            result.Value.Should().Be(pacienteCreadoEnHandler.Id);   // ← compare against the captured Paciente
            await _repositorio.Received(1).Agregar(Arg.Any<Paciente>());
            await _unidadDeTrabajo.Received(1).Persistir();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Handle_NombreInvalido_RetornaFailureYNoPersiste(string? nombre)
        {
            // Arrange
            var comando = new ComandoCrearPaciente { Nombre = nombre!, Email = "felipe@ejemplo.com" };

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Paciente.NombreObligatorio);
            await _repositorio.DidNotReceive().Agregar(Arg.Any<Paciente>());
            await _unidadDeTrabajo.DidNotReceive().Persistir();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Handle_EmailVacio_RetornaFailureEmailVacio(string? email)
        {
            // Arrange
            var comando = new ComandoCrearPaciente { Nombre = "Felipe", Email = email! };

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.Vacio);
            await _repositorio.DidNotReceive().Agregar(Arg.Any<Paciente>());
            await _unidadDeTrabajo.DidNotReceive().Persistir();
        }

        [Theory]
        [InlineData("EmailInvalido")]
        [InlineData("sin-arroba.com")]
        public async Task Handle_EmailFormatoInvalido_RetornaFailureFormatoInvalido(string? email)
        {
            // Arrange
            var comando = new ComandoCrearPaciente { Nombre = "Felipe", Email = email! };

            // Act
            var result = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Email.FormatoInvalido);
            await _repositorio.DidNotReceive().Agregar(Arg.Any<Paciente>());
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
            var comando = new ComandoCrearPaciente { Nombre = nombre!, Email = "felipe@ejemplo.com" };

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
            var comando = new ComandoCrearPaciente { Nombre = "Felipe", Email = email! };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Email);
        }

        [Fact]
        public void Validador_NombreYEmailValido_NoGeneraErrorDeValidacion()
        {
            // Arrange
            var comando = new ComandoCrearPaciente { Nombre = "Felipe", Email = "felipe@ejemplo.com" };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldNotHaveValidationErrorFor(c => c.Nombre);
            result.ShouldNotHaveValidationErrorFor(c => c.Email);
        }
    }
}
