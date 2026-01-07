using System;
using System.Collections.Generic;
using System.Text;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.ActualizarConsultorio;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.BorrarConsultorio;
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
    public class CasoDeUsoBorrarConsultorioTests
    {
        private readonly IRepositorioConsultorios _repositorio;
        private readonly IUnitOfwork _unidadDeTrabajo;
        private readonly HandlerBorrarConsultorio _handler;
        private readonly ValidadorComandoBorrarConsultorio _validator;

        public CasoDeUsoBorrarConsultorioTests()
        {
            _repositorio = Substitute.For<IRepositorioConsultorios>();
            _validator = new ValidadorComandoBorrarConsultorio();
            _unidadDeTrabajo = Substitute.For<IUnitOfwork>();

            _handler = new HandlerBorrarConsultorio(_repositorio, _unidadDeTrabajo);
        }

        // Primero hacemos las pruebas propias del Handler:

        [Fact]
        public async Task Handle_CuandoConsultorioExiste_BorraConsultorioYPersiste()
        {
            var id = Guid.NewGuid();
            var comando = new ComandoBorrarConsultorio { Id = id };
            var consultorio = new Consultorio("Consultorio A");

            _repositorio.ObtenerPorId(id).Returns(consultorio);

            await _handler.Handle(comando, CancellationToken.None);

            await _repositorio.Received(1).Borrar(consultorio);
            await _unidadDeTrabajo.Received(1).Persistir();

        }

        [Fact]
        public async Task Handle_CuandoConsultorioNoExiste_LanzaExcepcionNoEncontrado()
        {
            var comando = new ComandoBorrarConsultorio { Id = Guid.NewGuid() };
            _repositorio.ObtenerPorId(comando.Id).ReturnsNull();

            Func<Task> act = async () => await _handler.Handle(comando, CancellationToken.None);

            // Assert
            // Verify it throws the specific exception
            await act.Should().ThrowAsync<ExcepcionNoEncontrado>();
        }

        [Fact]
        public async Task Handle_CuandoOcurreExcepcion_LlamaAReversarYLanzaExcepcion()
        {
            var id = Guid.NewGuid();
            var comando = new ComandoBorrarConsultorio { Id = id };
            var consultorio = new Consultorio("Consultorio A");

            _repositorio.ObtenerPorId(id).Returns(consultorio);

            _repositorio.Borrar(consultorio).Throws((new InvalidOperationException("Fallo al borrar")));

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
            var comando = new ComandoBorrarConsultorio { Id = Guid.Empty};

            // Act
            var result = _validator.TestValidate(comando);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.Id);
        }

        [Fact]
        public void Validador_Valido()
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
