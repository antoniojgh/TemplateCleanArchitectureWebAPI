using DientesLimpios.Dominio.Entidades;
using DientesLimpios.Dominio.Enums;
using DientesLimpios.Dominio.Excepciones;
using DientesLimpios.Dominio.ObjetosDeValor;
using FluentAssertions; // Replaces Assert.AreEqual

namespace DientesLimpios.Tests.Dominio.Entidades
{
    public class CitaTests
    {
        // In xUnit, the class is re-instantiated for every test.
        // So these fields act as your 'Setup'.
        private readonly Guid _pacienteId = Guid.NewGuid();
        private readonly Guid _dentistaId = Guid.NewGuid();
        private readonly Guid _consultorioId = Guid.NewGuid();

        // We create a valid interval for general use
        private readonly IntervaloDeTiempo _intervalo = new IntervaloDeTiempo(
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(2)
        );

        [Fact]
        public void Constructor_CitaValida_EstadoEsProgramada()
        {
            // Act
            var cita = new Cita(_pacienteId, _dentistaId, _consultorioId, _intervalo);

            // Assert
            // We use .Should().Be() for value comparison
            cita.PacienteId.Should().Be(_pacienteId);
            cita.DentistaId.Should().Be(_dentistaId);
            cita.ConsultorioId.Should().Be(_consultorioId);
            cita.IntervaloDeTiempo.Should().Be(_intervalo);

            cita.Estado.Should().Be(EstadoCita.Programada);
            cita.Id.Should().NotBeEmpty();
        }

        [Fact]
        public void Constructor_FechaInicioEnElPasado_LanzaExcepcion()
        {
            // Arrange
            var intervaloPasado = new IntervaloDeTiempo(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

            // Act
            Action act = () => new Cita(_pacienteId, _dentistaId, _consultorioId, intervaloPasado);

            // Assert
            act.Should().Throw<ExcepcionDeReglaDeNegocio>();
        }

        [Fact]
        public void Cancelar_CitaProgramada_CambiaEstadoACancelada()
        {
            // Arrange
            var cita = new Cita(_pacienteId, _dentistaId, _consultorioId, _intervalo);

            // Act
            cita.Cancelar();

            // Assert
            cita.Estado.Should().Be(EstadoCita.Cancelada);
        }

        [Fact]
        public void Cancelar_CitaNoProgramada_LanzaExcepcion()
        {
            // Arrange
            var cita = new Cita(_pacienteId, _dentistaId, _consultorioId, _intervalo);
            cita.Cancelar(); // Now it is 'Cancelada'

            // Act
            Action act = () => cita.Cancelar(); // Trying to cancel again

            // Assert
            act.Should().Throw<ExcepcionDeReglaDeNegocio>();
        }

        [Fact]
        public void Completar_CitaProgramada_CambiaEstadoACompletada()
        {
            // Arrange
            var cita = new Cita(_pacienteId, _dentistaId, _consultorioId, _intervalo);

            // Act
            cita.Completar();

            // Assert
            cita.Estado.Should().Be(EstadoCita.Completada);
        }

        [Fact]
        public void Completar_CitaCancelada_LanzaExcepcion()
        {
            // Arrange
            var cita = new Cita(_pacienteId, _dentistaId, _consultorioId, _intervalo);
            cita.Cancelar();

            // Act
            Action act = () => cita.Completar();

            // Assert
            act.Should().Throw<ExcepcionDeReglaDeNegocio>();
        }
    }
}
