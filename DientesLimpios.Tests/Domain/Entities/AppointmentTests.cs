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
        private readonly DateTime _nowUtc;
        private readonly Guid _patientId;
        private readonly Guid _dentistId;
        private readonly Guid _officeId;
        private readonly TimeInterval _interval;

        public AppointmentTests()
        {
            _nowUtc = DateTime.UtcNow;
            _patientId = Guid.NewGuid();
            _dentistId = Guid.NewGuid();
            _officeId = Guid.NewGuid();
            // We create a valid interval for general use
            _interval = TimeInterval.Create(
                _nowUtc.AddDays(1),
                _nowUtc.AddDays(2)
            ).Value;
        }

        [Fact]
        public void Create_ValidAppointment_StatusIsScheduled()
        {
            // Act
            var appointmentResult = Appointment.Create(_patientId, _dentistId, _officeId, _interval.Start, _interval.End, _nowUtc);

            // Assert
            // We use .Should().Be() for value comparison
            appointmentResult.IsSuccess.Should().BeTrue();
            var appointment = appointmentResult.Value;

            appointment.PatientId.Should().Be(_patientId);
            appointment.DentistId.Should().Be(_dentistId);
            appointment.OfficeId.Should().Be(_officeId);
            appointment.TimeInterval.Should().Be(_interval);

            appointment.Status.Should().Be(AppointmentStatus.Scheduled);
            appointment.Id.Should().NotBeEmpty();
        }

        [Fact]
        public void Create_StartDateInThePast_ReturnsFailureInThePast()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-1);
            var endDate = DateTime.UtcNow.AddHours(-23);  // still after startDate;

            // Act
            var result = Appointment.Create(_patientId, _dentistId, _officeId, startDate, endDate, _nowUtc);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Appointment.InThePast);
        }

        [Fact]
        public void Cancel_ScheduledAppointment_ChangesStatusToCancelled()
        {
            // Arrange
            var appointmentResult = Appointment.Create(_patientId, _dentistId, _officeId, _interval.Start, _interval.End, _nowUtc);

            appointmentResult.IsSuccess.Should().BeTrue();
            var appointment = appointmentResult.Value;

            // Act
            var result = appointment.Cancel();

            // Assert
            result.IsSuccess.Should().BeTrue();
            appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        }

        [Fact]
        public void Cancel_AlreadyCancelledAppointment_ReturnsFailureOnlyScheduledCanBeCancelled()
        {
            // Arrange
            var appointmentResult = Appointment.Create(_patientId, _dentistId, _officeId, _interval.Start, _interval.End, _nowUtc);

            appointmentResult.IsSuccess.Should().BeTrue();
            var appointment = appointmentResult.Value;

            var firstCancelResult = appointment.Cancel(); // Now it is 'Cancelled'
            firstCancelResult.IsSuccess.Should().BeTrue();

            // Act
            var secondCancelResult = appointment.Cancel(); // Trying to cancel again


            // Assert
            secondCancelResult.IsFailure.Should().BeTrue();
            secondCancelResult.Error.Should().Be(DomainErrors.Appointment.OnlyScheduledCanBeCancelled);
        }

        [Fact]
        public void Complete_ScheduledAppointment_ChangesStatusToCompleted()
        {
            // Arrange
            var appointmentResult = Appointment.Create(_patientId, _dentistId, _officeId, _interval.Start, _interval.End, _nowUtc);

            appointmentResult.IsSuccess.Should().BeTrue();
            var appointment = appointmentResult.Value;

            // Act
            var result = appointment.Complete();

            // Assert
            result.IsSuccess.Should().BeTrue();
            appointment.Status.Should().Be(AppointmentStatus.Completed);
        }

        [Fact]
        public void Complete_CancelledAppointment_ReturnsFailure()
        {
            // Arrange
            var appointmentResult = Appointment.Create(_patientId, _dentistId, _officeId, _interval.Start, _interval.End, _nowUtc);

            appointmentResult.IsSuccess.Should().BeTrue();
            var appointment = appointmentResult.Value;

            var cancelResult = appointment.Cancel();
            cancelResult.IsSuccess.Should().BeTrue();

            // Act
            var completeResult = appointment.Complete();

            // Assert
            completeResult.IsFailure.Should().BeTrue();
            completeResult.Error.Should().Be(DomainErrors.Appointment.OnlyScheduledCanBeCompleted);
        }
    }
}
