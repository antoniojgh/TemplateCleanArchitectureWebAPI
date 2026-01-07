using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.CrearConsultorio;
using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using FluentAssertions; // For Should()
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.TestHelper;
using NSubstitute;      // For Substitute.For and Received()
using NSubstitute.ExceptionExtensions; // For Throws()

namespace DientesLimpios.Tests.Aplicacion.CasosDeUso.Consultorios
{
    public class CasoDeUsoCrearConsultorioTests
    {
        // Fields are private and readonly because they are set in the constructor
        // and never change during the test execution.
        private readonly IRepositorioConsultorios _repositorio;
        private readonly ValidadorComandoCrearConsultorio _validator;
        private readonly IUnitOfwork _unidadDeTrabajo;
        private readonly HandlerCrearConsultorio _handler;

        public CasoDeUsoCrearConsultorioTests()
        {
            _repositorio = Substitute.For<IRepositorioConsultorios>();
            _validator = new ValidadorComandoCrearConsultorio();
            _unidadDeTrabajo = Substitute.For<IUnitOfwork>();

            _handler = new HandlerCrearConsultorio(_repositorio, _unidadDeTrabajo);
        }

        // Primero hacemos las pruebas propias del Handler:

        [Fact]
        public async Task Handle_ComandoValido_ObtenemosIdConsultorio()
        {
            // Arrange
            var comando = new ComandoCrearConsultorio { Nombre = "Consultorio A" };

            var consultorioCreado = new Consultorio("Consultorio A");

            // Mocking the repository to return our object
            _repositorio.Agregar(Arg.Any<Consultorio>()).Returns(consultorioCreado);

            _unidadDeTrabajo.Persistir().Returns(Task.CompletedTask);

            // Act invocamos el handler del caso de uso
            var resultado = await _handler.Handle(comando, CancellationToken.None);

            // Assert
            await _repositorio.Received(1).Agregar(Arg.Any<Consultorio>());
            await _unidadDeTrabajo.Received(1).Persistir();

            // FluentAssertions replacement for Assert.AreNotEqual(Guid.Empty, ...)
            resultado.Should().NotBeEmpty();
        }


        [Fact]
        public async Task Handle_CuandoHayError_HacemosRollBack()
        {
            // Arrange
            var comando = new ComandoCrearConsultorio { Nombre = "Consultorio A" };

            // Mocking the repository to throw an exception
            _repositorio.Agregar(Arg.Any<Consultorio>()).Throws(new Exception("Error al crear el consultorio"));

            // Act, Invocamoes el handler del caso de uso
            Func<Task> act = async () => await _handler.Handle(comando, CancellationToken.None);

            // Assert
            // Verify it throws the specific exception
            await act.Should().ThrowAsync<Exception>();

            // Verify the repository was NEVER called because validation failed first
            await _unidadDeTrabajo.Received(1).Reversar();
        }

        // Luego hacemos las pruebas propias de la validacion, ya que el validador
        // es un componente externo al handler, ya no validamos dentro del Handler sino que lo hacemos medieante
        // un validador externo que se inyecta en el handler mediante la clase "ValidationBehavior"

        [Fact]
        public async Task Validador_NoValido()
        {
            // Arrange
            var comando = new ComandoCrearConsultorio { Nombre = "" };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Nombre);
        }

        [Fact]
        public void Validador_Valido()
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
