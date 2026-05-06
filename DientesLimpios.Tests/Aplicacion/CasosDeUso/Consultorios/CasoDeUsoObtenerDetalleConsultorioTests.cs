using DientesLimpios.Aplicacion.CasosdeUso.Consultorios.Consultas.ObtenerDetalleConsultorio;
using DientesLimpios.Aplicacion.Interfaces.Repositorios;
using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.Errores;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace DientesLimpios.Tests.Aplicacion.CasosDeUso.Consultorios
{
    public class CasoDeUsoObtenerDetalleConsultorioTests
    {
        private readonly IRepositorioConsultorios _repositorio;
        private readonly ILogger<HandlerObtenerDetalleConsultorio> _logger;
        private readonly HandlerObtenerDetalleConsultorio _handler;

        public CasoDeUsoObtenerDetalleConsultorioTests()
        {
            _repositorio = Substitute.For<IRepositorioConsultorios>();
            _logger = Substitute.For<ILogger<HandlerObtenerDetalleConsultorio>>();

            _handler = new HandlerObtenerDetalleConsultorio(_repositorio, _logger);
        }


        [Fact]
        public async Task Handle_ConsultorioExiste_RetornaDTO()
        {
            // Arrange
            var consultorioResult = Consultorio.Crear("Consultorio A");
            var consultorio = consultorioResult.Value;

            var id = consultorio.Id;
            var consulta = new ConsultaObtenerDetalleConsultorio { Id = id };

            _repositorio.ObtenerPorId(id).Returns(consultorio);

            // Act
            var result = await _handler.Handle(consulta, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(id);
            result.Value.Nombre.Should().Be("Consultorio A");
            await _repositorio.Received(1).ObtenerPorId(id);
        }

        [Fact]
        public async Task Handle_ConsultorioNoExiste_RetornaFailureNoEncontrado()
        {
            // Arrange
            var id = Guid.NewGuid();
            var consulta = new ConsultaObtenerDetalleConsultorio { Id = id };
            _repositorio.ObtenerPorId(id).ReturnsNull();

            // Act
            var result = await _handler.Handle(consulta, CancellationToken.None);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Consultorio.NoEncontrado);
            await _repositorio.Received(1).ObtenerPorId(id);
        }

    }
}
