using AutoMapper;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerListadoPacientes;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Mappings;
using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.ObjetosDeValor;
using FluentAssertions;
using NSubstitute;

namespace DientesLimpios.Tests.Aplicacion.CasosDeUso.Pacientes
{
    public class CasoDeUsoObtenerListadoPacientesTests
    {
        private readonly IRepositorioPacientes _repositorio;
        private readonly HandlerObtenerListadoPacientes _handler;
        // Use real IMapper, not a Mock
        private readonly IMapper _mapper;

        public CasoDeUsoObtenerListadoPacientesTests()
        {
            _repositorio = Substitute.For<IRepositorioPacientes>();

            // Create a REAL Mapper Configuration
            var config = new MapperConfiguration(cfg =>
            {
                // Load your actual profile
                cfg.AddProfile<MappingProfile>();
            });
            // Create the real mapper instance
            _mapper = config.CreateMapper();

            _handler = new HandlerObtenerListadoPacientes(_repositorio, _mapper);

        }

        [Fact]
        public async Task Handle_RetornaPacientesPaginadosCorrectamente()
        {
            var pagina = 1;
            var registrosPorPagina = 2;

            var filtroPacienteDTO = new FiltroPacienteDTO { Pagina = pagina, RegistrosPorPagina = registrosPorPagina };

            var paciente1 = new Paciente("Felipe", new Email("felipe@ejemplo.com"));
            var paciente2 = new Paciente("Claudia", new Email("claudia@ejemplo.com"));

            IEnumerable<Paciente> pacientes = new List<Paciente> { paciente1, paciente2 };

            _repositorio.ObtenerFiltrado(Arg.Any<FiltroPacienteDTO>()).Returns(Task.FromResult(pacientes));

            _repositorio.ObtenerCantidadTotalRegistros().Returns(Task.FromResult(10));

            var request = new ConsultaObtenerListadoPacientes
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
        public async Task Handle_CuandoNoHayPacientes_RetornaListaVaciaYTotalCero()
        {
            var pagina = 1;
            var registrosPorPagina = 5;

            var filtroPacienteDTO = new FiltroPacienteDTO { Pagina = pagina, RegistrosPorPagina = registrosPorPagina };

            IEnumerable<Paciente> pacientes = new List<Paciente>();

            _repositorio.ObtenerFiltrado(Arg.Any<FiltroPacienteDTO>()).Returns(Task.FromResult(pacientes));

            _repositorio.ObtenerCantidadTotalRegistros().Returns(Task.FromResult(0));

            var request = new ConsultaObtenerListadoPacientes
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
