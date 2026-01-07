using System.Reflection.Metadata;
using AutoMapper;
using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerDetalleConsultorio;
using DientesLimpios.Aplicacion.Excepciones;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Aplicacion.Mappings;
using DientesLimpios.Dominio.Entidades;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace DientesLimpios.Tests.Aplicacion.CasosDeUso.Consultorios
{
    public class CasoDeUsoObtenerDetalleConsultorioTests
    {
        private readonly IRepositorioConsultorios _repositorio;
        private readonly HandlerObtenerDetalleConsultorio _casoDeUso;
        // Use real IMapper, not a Mock
        private readonly IMapper _mapper;


        public CasoDeUsoObtenerDetalleConsultorioTests()
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

            _casoDeUso = new HandlerObtenerDetalleConsultorio(_repositorio, _mapper);
        }


        [Fact]
        public async Task Handle_ConsultorioExiste_RetornaDTO()
        {
            // Preparación
            var consultorio = new Consultorio("Consultorio A");
            var id = consultorio.Id;
            var consulta = new ConsultaObtenerDetalleConsultorio { Id = id };

            _repositorio.ObtenerPorId(id).Returns(consultorio);

            // Prueba
            var resultado = await _casoDeUso.Handle(consulta, CancellationToken.None);

            // Verificación
            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(id);
            resultado.Nombre.Should().Be("Consultorio A");
        }

        [Fact]
        public async Task Handle_ConsultorioNoExiste_LanzaExcepcionNoEncontrado()
        {
            // Preparación
            var id = Guid.NewGuid();
            var consulta = new ConsultaObtenerDetalleConsultorio { Id = id };
            _repositorio.ObtenerPorId(id).ReturnsNull();

            // Prueba
            // Act, Invocamoes el handler del caso de uso
            Func<Task> act = async () => await _casoDeUso.Handle(consulta, CancellationToken.None);

            // Assert
            // Verify it throws the specific validation exception
            await act.Should().ThrowAsync<ExcepcionNoEncontrado>();
        }

    }
}
