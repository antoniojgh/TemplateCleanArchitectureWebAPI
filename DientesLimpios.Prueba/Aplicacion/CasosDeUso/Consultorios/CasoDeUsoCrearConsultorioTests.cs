using System;
using System.Collections.Generic;
using System.Text;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Comandos.CrearConsultorio;
using DientesLimpios.Aplicacion.Interfaces.Persistencia;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using FluentValidation;
using MediatR;
using NSubstitute;

namespace DientesLimpios.Prueba.Aplicacion.CasosDeUso.Consultorios
{
    [TestClass]
    public class CasoDeUsoCrearConsultorioTests
    {
        private IRepositorioConsultorios _repositorioConsultorios;
        private IValidator<ComandoCrearConsultorio> _validador;
        private IUnitOfwork _unidadDeTrabajo;
        private IMediator _mediator;

        [TestInitialize]
        // Método que se ejecuta antes de cada prueba
        public void Setup()
        {
            // Aquí inicializarías los mocks o implementaciones concretas necesarias
            // para IRepositorioConsultorios, IValidator<ComandoCrearConsultorio>, IUnitOfwork y IMediator.

            // Ejemplo usando NSubstitute para crear mocks:
            _repositorioConsultorios = Substitute.For<IRepositorioConsultorios>();
            _validador = Substitute.For<IValidator<ComandoCrearConsultorio>>();
            _unidadDeTrabajo = Substitute.For<IUnitOfwork>();
            _mediator = Substitute.For<IMediator>();
        }



    }
}
