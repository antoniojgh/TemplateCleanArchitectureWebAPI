using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerListadoConsultorios;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DientesLimpios.Tests.Aplicacion.CasosDeUso.Consultorios
{
    public class CasoDeUsoObtenerListadoConsultoriosTests
    {
        private readonly IRepositorioConsultorios _repositorio;
        private readonly ILogger<HandlerObtenerListadoConsultorios> _logger;
        private readonly HandlerObtenerListadoConsultorios _handler;

        public CasoDeUsoObtenerListadoConsultoriosTests()
        {
            _repositorio = Substitute.For<IRepositorioConsultorios>();
            _logger = Substitute.For<ILogger<HandlerObtenerListadoConsultorios>>();

            _handler = new HandlerObtenerListadoConsultorios(_repositorio, _logger);
        }


        [Fact]
        public async Task Handle_CuandoHayConsultorios_RetornaListaDeConsultorioListadoDTO()
        {
            // Arrange
            var consultorios = new List<Consultorio>
                {
                    Consultorio.Crear("Consultorio A").Value,
                    Consultorio.Crear("Consultorio B").Value,
                };

            _repositorio.ObtenerTodos().Returns(consultorios);

            // Act
            var result = await _handler.Handle(new ConsultaObtenerListadoConsultorios(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Count.Should().Be(consultorios.Count);

            for (int i = 0; i < consultorios.Count; i++)
            {
                result.Value[i].Id.Should().Be(consultorios[i].Id);
                result.Value[i].Nombre.Should().Be(consultorios[i].Nombre);
            }
        }

        [Fact]
        public async Task Handle_CuandoNoHayConsultorios_RetornaListaVacia()
        {
            // Arrange
            _repositorio.ObtenerTodos().Returns(new List<Consultorio>());

            // Act
            var result = await _handler.Handle(new ConsultaObtenerListadoConsultorios(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Count.Should().Be(0);
            await _repositorio.Received(1).ObtenerTodos();
        }
    }
}
