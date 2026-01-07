using System;
using System.Collections.Generic;
using System.Text;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.ActualizarConsultorio;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.CrearConsultorio;
using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using FluentAssertions;
using FluentValidation.TestHelper;
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


        public CasoDeUsoActualizarConsultorioTests()
        {
            _repositorio = Substitute.For<IRepositorioConsultorios>();
            _validator = new ValidadorComandoActualizarConsultorio();
            _unidadDeTrabajo = Substitute.For<IUnitOfwork>();

            _handler = new HandlerActualizarConsultorio(_repositorio, _unidadDeTrabajo);
        }

        // Primero hacemos las pruebas propias del Handler:

        [Fact]
        public async Task Handle_CuandoConsultorioExiste_ActualizaNombreYPersiste()
        {
            var consultorio = new Consultorio("Consultorio A");
            var id = consultorio.Id;
            var comando = new ComandoActualizarConsultorio { Id = id, Nombre = "Nuevo nombre" };

            _repositorio.ObtenerPorId(id).Returns(consultorio);

            await _handler.Handle(comando, CancellationToken.None);

            await _repositorio.Received(1).Actualizar(consultorio);
            await _unidadDeTrabajo.Received(1).Persistir();

        }

        [Fact]
        public async Task Handle_CuandoConsultorioNoExiste_LanzaExcepcionNoEncontrado()
        {
            var comando = new ComandoActualizarConsultorio { Id = Guid.NewGuid(), Nombre = "Nombre" };
            _repositorio.ObtenerPorId(comando.Id).ReturnsNull();

            Func<Task> act = async () => await _handler.Handle(comando, CancellationToken.None);

            // Assert
            // Verify it throws the specific exception
            await act.Should().ThrowAsync<ExcepcionNoEncontrado>();
        }

        [Fact]
        public async Task Handle_CuandoOcurreExcepcionAlActualizar_LlamaAReversarYLanzaExcepcion()
        {
            var consultorio = new Consultorio("Consultorio A");
            var id = consultorio.Id;
            var comando = new ComandoActualizarConsultorio { Id = id, Nombre = "Consultorio B" };

            _repositorio.ObtenerPorId(id).Returns(consultorio);
            _repositorio.Actualizar(consultorio).Throws((new InvalidOperationException("Error al actualizar")));

            Func<Task> act = async () => await _handler.Handle(comando, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
            await _unidadDeTrabajo.Received(1).Reversar();
        }

        // Luego hacemos las pruebas propias de la validacion, ya que el validador
        // es un componente externo al handler, ya no validamos dentro del Handler sino que lo hacemos medieante
        // un validador externo que se inyecta en el handler mediante la clase "ValidationBehavior"

        [Fact]
        public async Task Validador_NoValido()
        {
            // Arrange
            var comando = new ComandoActualizarConsultorio { Id = Guid.NewGuid(), Nombre = "" };

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Nombre);
        }

        [Fact]
        public void Validador_Valido()
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
