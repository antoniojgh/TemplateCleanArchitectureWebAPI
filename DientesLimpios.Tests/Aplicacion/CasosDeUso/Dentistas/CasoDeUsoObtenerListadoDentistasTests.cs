
using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerListadoDentistas;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.ObjetosDeValor;
using FluentAssertions;
using NSubstitute;

namespace DientesLimpios.Tests.Aplicacion.CasosDeUso.Dentistas
{
    public class CasoDeUsoObtenerListadoDentistasTests
    {
        private readonly IRepositorioDentistas _repositorio;
        private readonly HandlerObtenerListadoDentistas _handler;

        public CasoDeUsoObtenerListadoDentistasTests()
        {
            _repositorio = Substitute.For<IRepositorioDentistas>();

            _handler = new HandlerObtenerListadoDentistas(_repositorio);

        }

        [Fact]
        public async Task Handle_RetornaDentistasPaginadosCorrectamente()
        {
            var pagina = 1;
            var registrosPorPagina = 2;

            var filtroDentistaDTO = new FiltroDentistaDTO { Pagina = pagina, RegistrosPorPagina = registrosPorPagina };

            var dentista1 = new Dentista("Felipe", new Email("felipe@ejemplo.com"));
            var dentista2 = new Dentista("Claudia", new Email("claudia@ejemplo.com"));

            IEnumerable<Dentista> dentistas = new List<Dentista> { dentista1, dentista2 };

            _repositorio.ObtenerFiltrado(Arg.Any<FiltroDentistaDTO>()).Returns(Task.FromResult(dentistas));

            _repositorio.ObtenerCantidadTotalRegistros().Returns(Task.FromResult(10));

            var request = new ConsultaObtenerListadoDentistas
            {
                Pagina = pagina,
                RegistrosPorPagina = registrosPorPagina
            };

            var resultado = await _handler.Handle(request, CancellationToken.None);

            resultado.Total.Should().Be(10);
            resultado.Elementos.Count.Should().Be(2);
            resultado.Elementos[0].Nombre.Should().Be("Felipe");
            resultado.Elementos[1].Nombre.Should().Be("Claudia");

        }

        [Fact]
        public async Task Handle_CuandoNoHayDentistas_RetornaListaVaciaYTotalCero()
        {
            var pagina = 1;
            var registrosPorPagina = 5;

            var filtroPacienteDTO = new FiltroDentistaDTO { Pagina = pagina, RegistrosPorPagina = registrosPorPagina };

            IEnumerable<Dentista> dentistas = new List<Dentista>();

            _repositorio.ObtenerFiltrado(Arg.Any<FiltroDentistaDTO>()).Returns(Task.FromResult(dentistas));

            _repositorio.ObtenerCantidadTotalRegistros().Returns(Task.FromResult(0));

            var request = new ConsultaObtenerListadoDentistas
            {
                Pagina = pagina,
                RegistrosPorPagina = registrosPorPagina
            };

            var resultado = await _handler.Handle(request, CancellationToken.None);

            resultado.Total.Should().Be(0);
            resultado.Elementos.Should().NotBeNull();
            resultado.Elementos.Count.Should().Be(0);
        }
    }
}
