using DientesLimpios.Aplicacion.CasosdeUso.Dentistas.Consultas.ObtenerListadoDentistas;
using DientesLimpios.Aplicacion.CasosdeUso.Pacientes.Consultas.ObtenerListadoPacientes;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace DientesLimpios.Tests.Aplicacion.CasosDeUso.Pacientes
{
    public class CasoDeUsoObtenerListadoPacientesTests
    {
        private readonly IRepositorioPacientes _repositorio;
        private readonly ILogger<HandlerObtenerListadoPacientes> _logger;
        private readonly HandlerObtenerListadoPacientes _handler;

        public CasoDeUsoObtenerListadoPacientesTests()
        {
            _repositorio = Substitute.For<IRepositorioPacientes>();
            _logger = Substitute.For<ILogger<HandlerObtenerListadoPacientes>>();

            _handler = new HandlerObtenerListadoPacientes(_repositorio, _logger);
        }

        [Fact]
        public async Task Handle_CuandoHayPacientes_RetornaPaginadoConDTOsCorrectos()
        {
            // Arrange
            var pagina = 1;
            var registrosPorPagina = 2;

            var paciente1 = Paciente.Crear("Felipe", "felipe@ejemplo.com").Value;
            var paciente2 = Paciente.Crear("Claudia", "claudia@ejemplo.com").Value;

            var pacientes = new List<Paciente> { paciente1, paciente2 };

            _repositorio.ObtenerFiltrado(Arg.Any<FiltroPacienteDTO>()).Returns(pacientes);

            _repositorio.ObtenerCantidadTotalRegistros().Returns(10);

            var request = new ConsultaObtenerListadoPacientes
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
            await _repositorio.Received(1).ObtenerFiltrado(Arg.Any<FiltroPacienteDTO>());
            await _repositorio.Received(1).ObtenerCantidadTotalRegistros();

        }

        [Fact]
        public async Task Handle_CuandoNoHayPacientes_RetornaListaVaciaYTotalCero()
        {
            // Arrange
            var pagina = 1;
            var registrosPorPagina = 5;

            var filtroPacienteDTO = new FiltroPacienteDTO { Pagina = pagina, RegistrosPorPagina = registrosPorPagina };

            IEnumerable<Paciente> pacientes = new List<Paciente>();

            _repositorio.ObtenerFiltrado(Arg.Any<FiltroPacienteDTO>()).Returns(pacientes);

            _repositorio.ObtenerCantidadTotalRegistros().Returns(0);

            var request = new ConsultaObtenerListadoPacientes
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
