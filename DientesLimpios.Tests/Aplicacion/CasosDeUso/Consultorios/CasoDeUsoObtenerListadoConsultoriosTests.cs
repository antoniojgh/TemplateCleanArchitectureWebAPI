using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerListadoConsultorios;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using FluentAssertions;
using NSubstitute;

namespace DientesLimpios.Tests.Aplicacion.CasosDeUso.Consultorios
{
    public class CasoDeUsoObtenerListadoConsultoriosTests
    {
        private readonly IRepositorioConsultorios _repositorio;
        private readonly HandlerObtenerListadoConsultorios _casoDeUso;

        public CasoDeUsoObtenerListadoConsultoriosTests()
        {
            _repositorio = Substitute.For<IRepositorioConsultorios>();

            _casoDeUso = new HandlerObtenerListadoConsultorios(_repositorio);
        }


        [Fact]
        public async Task Handle_CuandoHayConsultorios_RetornaListaDeConsultorioListadoDTO()
        {
            var consultorios = new List<Consultorio>
                {
                    new Consultorio( "Consultorio A"),
                    new Consultorio( "Consultorio B"),
                };

            _repositorio.ObtenerTodos().Returns(consultorios);

            var esperado = consultorios.Select(consultorio => consultorio.ADto()).ToList();

            var resultado = await _casoDeUso.Handle(new ConsultaObtenerListadoConsultorios());


            // Verificación:

            resultado.Count.Should().Be(esperado.Count);

            for (int i = 0; i < esperado.Count; i++)
            {
                resultado[i].Id.Should().Be(esperado[i].Id);
                resultado[i].Nombre.Should().Be(esperado[i].Nombre);
            }
        }

        [Fact]
        public async Task Handle_CuandoNoHayConsultorios_RetornaListaVacia()
        {
            _repositorio.ObtenerTodos().Returns(new List<Consultorio>());

            var resultado = await _casoDeUso.Handle(new ConsultaObtenerListadoConsultorios());

            resultado.Should().NotBeNull();
            resultado.Count.Should().Be(0);
        }
    }
}
