using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.Enums;
using DientesLimpios.Domain.Errors;
using DientesLimpios.Domain.Events;
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
            // A fixed instant, not the real clock: Domain receives "now" as a parameter, so
            // the tests can pin it and assert on it exactly.
            _nowUtc = new DateTime(2026, 9, 16, 6, 0, 0, DateTimeKind.Utc);
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
        public void Create_OnSuccess_RaisesAppointmentCreatedEvent()
        {
            // Act
            var appointmentResult = Appointment.Create(_patientId, _dentistId, _officeId, _interval.Start, _interval.End, _nowUtc);

            // Assert
            appointmentResult.IsSuccess.Should().BeTrue();
            var appointment = appointmentResult.Value;

            var domainEvent = appointment.DomainEvents
                .OfType<AppointmentCreatedEvent>()
                .Should().ContainSingle()
                .Subject;

            domainEvent.AppointmentId.Should().Be(appointment.Id);
            domainEvent.PatientId.Should().Be(_patientId);
            domainEvent.DentistId.Should().Be(_dentistId);
            domainEvent.OfficeId.Should().Be(_officeId);
            domainEvent.StartDate.Should().Be(_interval.Start);
            domainEvent.EndDate.Should().Be(_interval.End);
        }

        [Fact]
        public void Create_StartDateInThePast_ReturnsFailureInThePast()
        {
            // Arrange
            var startDate = _nowUtc.AddDays(-1);
            var endDate = _nowUtc.AddHours(-23);  // still after startDate;

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
            var result = appointment.Cancel(_nowUtc);

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

            var firstCancelResult = appointment.Cancel(_nowUtc); // Now it is 'Cancelled'
            firstCancelResult.IsSuccess.Should().BeTrue();

            // Act
            var secondCancelResult = appointment.Cancel(_nowUtc); // Trying to cancel again


            // Assert
            secondCancelResult.IsFailure.Should().BeTrue();
            secondCancelResult.Error.Should().Be(DomainErrors.Appointment.OnlyScheduledCanBeCancelled);
        }

        [Fact]
        public void Cancel_ScheduledAppointment_RaisesAppointmentCancelledEvent()
        {
            // Arrange — cancel at a different instant from creation, so only the value passed
            // to Cancel can match OccurredOnUtc.
            var appointment = Appointment.Create(_patientId, _dentistId, _officeId,
                _interval.Start, _interval.End, _nowUtc).Value;
            var cancelledAtUtc = _nowUtc.AddHours(1);

            // Act
            appointment.Cancel(cancelledAtUtc).IsSuccess.Should().BeTrue();

            // Assert
            var domainEvent = appointment.DomainEvents
                .OfType<AppointmentCancelledEvent>()
                .Should().ContainSingle()
                .Subject;

            domainEvent.AppointmentId.Should().Be(appointment.Id);
            domainEvent.PatientId.Should().Be(_patientId);
            domainEvent.StartDate.Should().Be(_interval.Start);
            domainEvent.OccurredOnUtc.Should().Be(cancelledAtUtc);
        }

        [Fact]
        public void Cancel_AlreadyCancelledAppointment_RaisesNoSecondEvent()
        {
            // Arrange
            var appointment = Appointment.Create(_patientId, _dentistId, _officeId,
                _interval.Start, _interval.End, _nowUtc).Value;

            appointment.Cancel(_nowUtc).IsSuccess.Should().BeTrue();

            // Act
            appointment.Cancel(_nowUtc.AddHours(1)).IsFailure.Should().BeTrue();

            // Assert — the rejected second call leaves only the first event.
            appointment.DomainEvents.OfType<AppointmentCancelledEvent>().Should().ContainSingle();
        }

        [Fact]
        public void Complete_ScheduledAppointment_ChangesStatusToCompleted()
        {
            // Arrange
            var appointmentResult = Appointment.Create(_patientId, _dentistId, _officeId, _interval.Start, _interval.End, _nowUtc);

            appointmentResult.IsSuccess.Should().BeTrue();
            var appointment = appointmentResult.Value;

            // Act
            var result = appointment.Complete(_nowUtc);

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

            var cancelResult = appointment.Cancel(_nowUtc);
            cancelResult.IsSuccess.Should().BeTrue();

            // Act
            var completeResult = appointment.Complete(_nowUtc);

            // Assert
            completeResult.IsFailure.Should().BeTrue();
            completeResult.Error.Should().Be(DomainErrors.Appointment.OnlyScheduledCanBeCompleted);
        }

        [Fact]
        public void MarkConfirmationSent_FirstTime_SetsConfirmationSentAtUtc()
        {
            // Arrange
            var appointment = Appointment.Create(_patientId, _dentistId, _officeId,
                _interval.Start, _interval.End, _nowUtc).Value;
            var sentAt = _nowUtc.AddMinutes(5);

            // Act
            var result = appointment.MarkConfirmationSent(sentAt);

            // Assert
            result.IsSuccess.Should().BeTrue();
            appointment.ConfirmationSentAtUtc.Should().Be(sentAt);
        }

        [Fact]
        public void MarkConfirmationSent_AlreadySent_ReturnsFailureConfirmationAlreadySent()
        {
            // Arrange
            var appointment = Appointment.Create(_patientId, _dentistId, _officeId,
                _interval.Start, _interval.End, _nowUtc).Value;

            appointment.MarkConfirmationSent(_nowUtc).IsSuccess.Should().BeTrue();

            // Act
            var result = appointment.MarkConfirmationSent(_nowUtc.AddMinutes(5));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(DomainErrors.Appointment.ConfirmationAlreadySent);
            appointment.ConfirmationSentAtUtc.Should().Be(_nowUtc);   // the first time wins
        }

        [Fact]
        public void Create_OnSuccess_EventOccursAtTheGivenInstant()
        {
            // Arrange — an instant far from the real clock, so only a value passed in can match.
            var nowUtc = new DateTime(2020, 1, 1, 9, 0, 0, DateTimeKind.Utc);

            // Act
            var appointment = Appointment.Create(_patientId, _dentistId, _officeId,
                _interval.Start, _interval.End, nowUtc).Value;

            // Assert
            appointment.DomainEvents
                .OfType<AppointmentCreatedEvent>()
                .Should().ContainSingle()
                .Which.OccurredOnUtc.Should().Be(nowUtc);
        }
    }
}
