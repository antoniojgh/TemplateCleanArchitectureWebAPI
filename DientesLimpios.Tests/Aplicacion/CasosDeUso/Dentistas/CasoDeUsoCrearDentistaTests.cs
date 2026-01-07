using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Comandos.CrearDentista;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Comandos.CrearPaciente;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.ObjetosDeValor;
using FluentAssertions;
using FluentValidation.TestHelper;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace DientesLimpios.Tests.Aplicacion.CasosDeUso.Dentistas
{
    public class CasoDeUsoCrearDentistaTests
    {
        private readonly IRepositorioDentistas _repositorio;
        private readonly IUnitOfwork _unidadDeTrabajo;
        private readonly HandlerCrearDentista _handler;
        private readonly ValidadorComandoCrearDentista _validator;

        public CasoDeUsoCrearDentistaTests()
        {
            // Aquí deberías inicializar los mocks o stubs necesarios para las pruebas
            // Por ejemplo, podrías usar Moq para crear un mock de IRepositorioPacientes
            // y IUnitOfwork, y luego pasarlos al handler y al validador.

            _repositorio = Substitute.For<IRepositorioDentistas>();
            _validator = new ValidadorComandoCrearDentista();
            _unidadDeTrabajo = Substitute.For<IUnitOfwork>();

            _handler = new HandlerCrearDentista(_repositorio, _unidadDeTrabajo);
        }

        // Primero hacemos las pruebas propias del Handler:

        [Fact]
        public async Task Handle_CuandoDatosValidos_CreaDentistaYPersisteYRetornaId()
        {
            var comando = new ComandoCrearDentista { Nombre = "Felipe", Email = "felipe@ejemplo.com" };
            var dentistaCreado = new Dentista(comando.Nombre, new Email(comando.Email));
            var id = dentistaCreado.Id;

            _repositorio.Agregar(Arg.Any<Dentista>()).Returns(dentistaCreado);

            var idResultado = await _handler.Handle(comando, CancellationToken.None);

            idResultado.Should().Be(id);
            await _repositorio.Received(1).Agregar(Arg.Any<Dentista>());
            await _unidadDeTrabajo.Received(1).Persistir();

        }

        [Fact]
        public async Task Handle_CuandoOcurreExcepcion_ReversarYLanzaExcepcion()
        {
            var comando = new ComandoCrearDentista { Nombre = "Felipe", Email = "felipe@ejemplo.com" };
            _repositorio.Agregar(Arg.Any<Dentista>()).Throws(new InvalidOperationException("Error al insertar"));

            Func<Task> act = async () => await _handler.Handle(comando, CancellationToken.None);

            // Assert
            // Verify it throws the specific exception
            await act.Should().ThrowAsync<InvalidOperationException>();

            await _unidadDeTrabajo.Received(1).Reversar();
            await _unidadDeTrabajo.DidNotReceive().Persistir();
        }

        // Luego hacemos las pruebas propias de la validacion, ya que el validador
        // es un componente externo al handler, ya no validamos dentro del Handler sino que lo hacemos medieante
        // un validador externo que se inyecta en el handler mediante la clase "ValidationBehavior"

        [Fact]
        public async Task Validador_NoValido()
        {
            // Arrange
            var comando = new ComandoCrearDentista { Nombre = "", Email = "felipe" };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Nombre);
            result.ShouldHaveValidationErrorFor(c => c.Email);
        }

        [Fact]
        public void Validador_Valido()
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
