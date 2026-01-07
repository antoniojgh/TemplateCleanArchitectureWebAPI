using AutoMapper;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerListadoConsultorios;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Mappings;
using DientesLimpios.Dominio.Entidades;
using FluentAssertions;
using NSubstitute;

namespace DientesLimpios.Tests.Aplicacion.CasosDeUso.Consultorios
{
    public class CasoDeUsoObtenerListadoConsultoriosTests
    {
        private readonly IRepositorioConsultorios _repositorio;
        private readonly HandlerObtenerListadoConsultorios _casoDeUso;
        // Use real IMapper, not a Mock
        private readonly IMapper _mapper;


        public CasoDeUsoObtenerListadoConsultoriosTests()
        {
            _repositorio = Substitute.For<IRepositorioConsultorios>();

            // Create a REAL Mapper Configuration
            var config = new MapperConfiguration(cfg =>
            {
                // Load your actual profile
                cfg.AddProfile<MappingProfile>();
            });
            // Create the real mapper instance
            _mapper = config.CreateMapper();

            _casoDeUso = new HandlerObtenerListadoConsultorios(_repositorio, _mapper);
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

            var esperado = _mapper.Map<List<ConsultorioListadoDTO>>(consultorios);

            var resultado = await _casoDeUso.Handle(new ConsultaObtenerListadoConsultorios(), CancellationToken.None);


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

            var resultado = await _casoDeUso.Handle(new ConsultaObtenerListadoConsultorios(), CancellationToken.None);

            resultado.Should().NotBeNull();
            resultado.Count.Should().Be(0);
        }
    }
}
