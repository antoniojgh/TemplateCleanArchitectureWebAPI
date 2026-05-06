using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerListadoDentistas;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DientesLimpios.Tests.Aplicacion.CasosDeUso.Dentistas
{
    public class CasoDeUsoObtenerListadoDentistasTests
    {
        private readonly IRepositorioDentistas _repositorio;
        private readonly ILogger<HandlerObtenerListadoDentistas> _logger;
        private readonly HandlerObtenerListadoDentistas _handler;

        public CasoDeUsoObtenerListadoDentistasTests()
        {
            _repositorio = Substitute.For<IRepositorioDentistas>();
            _logger = Substitute.For<ILogger<HandlerObtenerListadoDentistas>>();

            _handler = new HandlerObtenerListadoDentistas(_repositorio, _logger);

        }

        [Fact]
        public async Task Handle_CuandoHayDentistas_RetornaPaginadoConDTOsCorrectos()
        {
            // Arrange
            var pagina = 1;
            var registrosPorPagina = 2;

            var dentista1 = Dentista.Crear("Felipe", "felipe@ejemplo.com").Value;
            var dentista2 = Dentista.Crear("Claudia", "claudia@ejemplo.com").Value;

            var dentistas = new List<Dentista> { dentista1, dentista2 };

            _repositorio.ObtenerFiltrado(Arg.Any<FiltroDentistaDTO>()).Returns(dentistas);

            _repositorio.ObtenerCantidadTotalRegistros().Returns(10);

            var request = new ConsultaObtenerListadoDentistas
            {
                Pagina = pagina,
                RegistrosPorPagina = registrosPorPagina
            };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Total.Should().Be(10);
            result.Value.Elementos.Count.Should().Be(2);
            result.Value.Elementos[0].Nombre.Should().Be("Felipe");
            result.Value.Elementos[0].Email.Should().Be("felipe@ejemplo.com");
            result.Value.Elementos[1].Nombre.Should().Be("Claudia");
            result.Value.Elementos[1].Email.Should().Be("claudia@ejemplo.com");
            await _repositorio.Received(1).ObtenerFiltrado(Arg.Any<FiltroDentistaDTO>());
            await _repositorio.Received(1).ObtenerCantidadTotalRegistros();
        }

        [Fact]
        public async Task Handle_CuandoNoHayDentistas_RetornaListaVaciaYTotalCero()
        {
            // Arrange
            var pagina = 1;
            var registrosPorPagina = 5;

            var filtroPacienteDTO = new FiltroDentistaDTO { Pagina = pagina, RegistrosPorPagina = registrosPorPagina };

            IEnumerable<Dentista> dentistas = new List<Dentista>();

            _repositorio.ObtenerFiltrado(Arg.Any<FiltroDentistaDTO>()).Returns(dentistas);

            _repositorio.ObtenerCantidadTotalRegistros().Returns(0);

            var request = new ConsultaObtenerListadoDentistas
            {
                Pagina = pagina,
                RegistrosPorPagina = registrosPorPagina
            };

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Total.Should().Be(0);
            result.Value.Elementos.Count.Should().Be(0);
        }
    }
}
