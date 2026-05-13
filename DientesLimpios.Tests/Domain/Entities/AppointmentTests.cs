using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Enums;
using DientesLimpios.Domain.Errors;
using DientesLimpios.Domain.ValueObjects;
using FluentAssertions; // Replaces Assert.AreEqual

namespace DientesLimpios.Tests.Domain.Entities
{
    public class AppointmentTests
    {
        // In xUnit, the class is re-instantiated for every test.
        // So these fields act as your 'Setup'.
        private readonly DateTime _ahoraUtc;
        private readonly Guid _pacienteId;
        private readonly Guid _dentistaId;
        private readonly Guid _consultorioId;
        private readonly TimeInterval _intervalo;

        public AppointmentTests()
        {
            _ahoraUtc = DateTime.UtcNow;
            _pacienteId = Guid.NewGuid();
            _dentistaId = Guid.NewGuid();
            _consultorioId = Guid.NewGuid();
            // We create a valid interval for general use
            _intervalo = TimeInterval.Create(
                _ahoraUtc.AddDays(1),
                _ahoraUtc.AddDays(2)
            ).Value;
        }

        [Fact]
        public void Crear_AppointmentValida_EstadoEsScheduled()
        {
            // Act
            var appointmentResult = Appointment.Create(_pacienteId, _dentistaId, _consultorioId, _intervalo.Start, _intervalo.End, _ahoraUtc);

            // Assert
            // We use .Should().Be() for value comparison
            appointmentResult.IsSuccess.Should().BeTrue();
            var appointment = appointmentResult.Value;

            appointment.PatientId.Should().Be(_pacienteId);
            appointment.DentistId.Should().Be(_dentistaId);
            appointment.OfficeId.Should().Be(_consultorioId);
            appointment.TimeInterval.Should().Be(_intervalo);

            appointment.Status.Should().Be(AppointmentStatus.Scheduled);
            appointment.Id.Should().NotBeEmpty();
        }

        [Fact]
        public void Crear_FechaInicioInThePast_RetornaFailureInThePast()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-1);
            var endDate = DateTime.UtcNow.AddHours(-23);  // still after startDate;

            // Act
            var result = Appointment.Create(_pacienteId, _dentistaId, _consultorioId, startDate, endDate, _ahoraUtc);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Appointment.InThePast);
        }

        [Fact]
        public void Cancelar_AppointmentScheduled_CambiaEstadoACancelled()
        {
            // Arrange
            var appointmentResult = Appointment.Create(_pacienteId, _dentistaId, _consultorioId, _intervalo.Start, _intervalo.End, _ahoraUtc);
            
            appointmentResult.IsSuccess.Should().BeTrue();
            var appointment = appointmentResult.Value;

            // Act
            var result = appointment.Cancel();

            // Assert
            result.IsSuccess.Should().BeTrue();
            appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        }

        [Fact]
        public void Cancelar_AppointmentYaCancelled_RetornaFailureOnlyScheduledCanBeCancelled()
        {
            // Arrange
            var appointmentResult = Appointment.Create(_pacienteId, _dentistaId, _consultorioId, _intervalo.Start, _intervalo.End, _ahoraUtc);

            appointmentResult.IsSuccess.Should().BeTrue();
            var appointment = appointmentResult.Value;

            var citaCancelarResult = appointment.Cancel(); // Now it is 'Cancelled'
            citaCancelarResult.IsSuccess.Should().BeTrue();

            // Act
            var citaCancelledResult = appointment.Cancel(); // Trying to cancel again


            // Assert
            citaCancelledResult.IsFailure.Should().BeTrue();
            citaCancelledResult.Error.Should().Be(DomainErrors.Appointment.OnlyScheduledCanBeCancelled);
        }

        [Fact]
        public void Completar_AppointmentScheduled_CambiaEstadoACompleted()
        {
            // Arrange
            var appointmentResult = Appointment.Create(_pacienteId, _dentistaId, _consultorioId, _intervalo.Start, _intervalo.End, _ahoraUtc);

            appointmentResult.IsSuccess.Should().BeTrue();
            var appointment = appointmentResult.Value;

            // Act
            var result = appointment.Complete();

            // Assert
            result.IsSuccess.Should().BeTrue();
            appointment.Status.Should().Be(AppointmentStatus.Completed);
        }

        [Fact]
        public void Completar_AppointmentCancelled_RetornaFailure()
        {
            // Arrange
            var appointmentResult = Appointment.Create(_pacienteId, _dentistaId, _consultorioId, _intervalo.Start, _intervalo.End, _ahoraUtc);

            appointmentResult.IsSuccess.Should().BeTrue();
            var appointment = appointmentResult.Value;

            var citaCancelarResult = appointment.Cancel();
            citaCancelarResult.IsSuccess.Should().BeTrue();

            // Act
            var resultCompletar = appointment.Complete();

            // Assert
            resultCompletar.IsFailure.Should().BeTrue();
            resultCompletar.Error.Should().Be(DomainErrors.Appointment.OnlyScheduledCanBeCompleted);
        }
    }
}
