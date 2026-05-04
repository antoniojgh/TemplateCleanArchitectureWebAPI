using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.Enums;
using DientesLimpios.Dominio.Errores;
using DientesLimpios.Dominio.ObjetosDeValor;
using FluentAssertions; // Replaces Assert.AreEqual

namespace DientesLimpios.Tests.Dominio.Entidades
{
    public class CitaTests
    {
        // In xUnit, the class is re-instantiated for every test.
        // So these fields act as your 'Setup'.
        private readonly DateTime _ahoraUtc;
        private readonly Guid _pacienteId;
        private readonly Guid _dentistaId;
        private readonly Guid _consultorioId;
        private readonly IntervaloDeTiempo _intervalo;

        public CitaTests()
        {
            _ahoraUtc = DateTime.UtcNow;
            _pacienteId = Guid.NewGuid();
            _dentistaId = Guid.NewGuid();
            _consultorioId = Guid.NewGuid();
            // We create a valid interval for general use
            _intervalo = IntervaloDeTiempo.Crear(
                _ahoraUtc.AddDays(1),
                _ahoraUtc.AddDays(2)
            ).Value;
        }

        [Fact]
        public void Crear_CitaValida_EstadoEsProgramada()
        {
            // Act
            var citaResult = Cita.Crear(_pacienteId, _dentistaId, _consultorioId, _intervalo.Inicio, _intervalo.Fin, _ahoraUtc);

            // Assert
            // We use .Should().Be() for value comparison
            citaResult.IsSuccess.Should().BeTrue();
            var cita = citaResult.Value;

            cita.PacienteId.Should().Be(_pacienteId);
            cita.DentistaId.Should().Be(_dentistaId);
            cita.ConsultorioId.Should().Be(_consultorioId);
            cita.IntervaloDeTiempo.Should().Be(_intervalo);

            cita.Estado.Should().Be(EstadoCita.Programada);
            cita.Id.Should().NotBeEmpty();
        }

        [Fact]
        public void Crear_FechaInicioEnElPasado_RetornaFailureEnElPasado()
        {
            // Arrange
            var fechaInicio = DateTime.UtcNow.AddDays(-1);
            var fechaFin = DateTime.UtcNow.AddHours(-23);  // still after fechaInicio;

            // Act
            var result = Cita.Crear(_pacienteId, _dentistaId, _consultorioId, fechaInicio, fechaFin, _ahoraUtc);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Cita.EnElPasado);
        }

        [Fact]
        public void Cancelar_CitaProgramada_CambiaEstadoACancelada()
        {
            // Arrange
            var citaResult = Cita.Crear(_pacienteId, _dentistaId, _consultorioId, _intervalo.Inicio, _intervalo.Fin, _ahoraUtc);
            
            citaResult.IsSuccess.Should().BeTrue();
            var cita = citaResult.Value;

            // Act
            var result = cita.Cancelar();

            // Assert
            result.IsSuccess.Should().BeTrue();
            cita.Estado.Should().Be(EstadoCita.Cancelada);
        }

        [Fact]
        public void Cancelar_CitaYaCancelada_RetornaFailureSoloSePuedenCancelarProgramadas()
        {
            // Arrange
            var citaResult = Cita.Crear(_pacienteId, _dentistaId, _consultorioId, _intervalo.Inicio, _intervalo.Fin, _ahoraUtc);

            citaResult.IsSuccess.Should().BeTrue();
            var cita = citaResult.Value;

            var citaCancelarResult = cita.Cancelar(); // Now it is 'Cancelada'
            citaCancelarResult.IsSuccess.Should().BeTrue();

            // Act
            var citaCanceladaResult = cita.Cancelar(); // Trying to cancel again


            // Assert
            citaCanceladaResult.IsFailure.Should().BeTrue();
            citaCanceladaResult.Error.Should().Be(DomainErrors.Cita.SoloSePuedenCancelarProgramadas);
        }

        [Fact]
        public void Completar_CitaProgramada_CambiaEstadoACompletada()
        {
            // Arrange
            var citaResult = Cita.Crear(_pacienteId, _dentistaId, _consultorioId, _intervalo.Inicio, _intervalo.Fin, _ahoraUtc);

            citaResult.IsSuccess.Should().BeTrue();
            var cita = citaResult.Value;

            // Act
            var result = cita.Completar();

            // Assert
            result.IsSuccess.Should().BeTrue();
            cita.Estado.Should().Be(EstadoCita.Completada);
        }

        [Fact]
        public void Completar_CitaCancelada_RetornaFailure()
        {
            // Arrange
            var citaResult = Cita.Crear(_pacienteId, _dentistaId, _consultorioId, _intervalo.Inicio, _intervalo.Fin, _ahoraUtc);

            citaResult.IsSuccess.Should().BeTrue();
            var cita = citaResult.Value;

            var citaCancelarResult = cita.Cancelar();
            citaCancelarResult.IsSuccess.Should().BeTrue();

            // Act
            var resultCompletar = cita.Completar();

            // Assert
            resultCompletar.IsFailure.Should().BeTrue();
            resultCompletar.Error.Should().Be(DomainErrors.Cita.SoloSePuedenCompletarProgramadas);
        }
    }
}
