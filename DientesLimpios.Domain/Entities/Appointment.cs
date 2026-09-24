using DientesLimpios.Domain.Common;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Enums;
using DientesLimpios.Domain.Errors;
using DientesLimpios.Domain.ValueObjects;
using DientesLimpios.Domain.Events;

namespace DientesLimpios.Domain.Entities
{
    public class Appointment : AggregateRoot
    {
        public Guid PatientId { get; private set; }
        public Guid DentistId { get; private set; }
        public Guid OfficeId { get; private set; }
        public AppointmentStatus Status { get; private set; }
        public TimeInterval TimeInterval { get; private set; } = null!;
        public DateTime? ConfirmationSentAtUtc { get; private set; }
        public DateTime? CancellationSentAtUtc { get; private set; }

        // Keyed by event, not by time: an appointment can be rescheduled many times, and each
        // reschedule gets its own email.
        public Guid? RescheduleNoticeEventId { get; private set; }
        public DateTime? CompletedAtUtc { get; private set; }

        private Appointment() { }   // EF Core

        private Appointment(
            Guid patientId, Guid dentistId, Guid officeId,
            TimeInterval timeInterval) : base(Guid.CreateVersion7())
        {
            PatientId = patientId;
            DentistId = dentistId;
            OfficeId = officeId;
            TimeInterval = timeInterval;
            Status = AppointmentStatus.Scheduled;
        }

        public static Result<Appointment> Create(
            Guid patientId, Guid dentistId, Guid officeId,
            DateTime startDate, DateTime endDate, DateTime nowUtc)
        {
            if (startDate < nowUtc)
                return Result.Failure<Appointment>(DomainErrors.Appointment.InThePast);

            var intervalResult = TimeInterval.Create(startDate, endDate);
            if (intervalResult.IsFailure)
                return Result.Failure<Appointment>(intervalResult.Error);

            var appointment = new Appointment(
                patientId, dentistId, officeId, intervalResult.Value);

            appointment.RaiseDomainEvent(new AppointmentCreatedEvent(
                appointment.Id, patientId, dentistId, officeId, startDate, endDate, nowUtc));


            return Result.Success(appointment);
        }

        public Result Cancel(DateTime nowUtc)
        {
            if (Status != AppointmentStatus.Scheduled)
                return Result.Failure(DomainErrors.Appointment.OnlyScheduledCanBeCancelled);

            Status = AppointmentStatus.Cancelled;

            RaiseDomainEvent(new AppointmentCancelledEvent(Id, PatientId, TimeInterval.Start, nowUtc));

            return Result.Success();
        }

        public Result Complete(DateTime nowUtc)
        {
            if (Status != AppointmentStatus.Scheduled)
                return Result.Failure(DomainErrors.Appointment.OnlyScheduledCanBeCompleted);

            Status = AppointmentStatus.Completed;
            CompletedAtUtc = nowUtc;
            return Result.Success();
        }

        public Result Reschedule(DateTime newStartDate, DateTime newEndDate, DateTime nowUtc)
        {
            if (Status != AppointmentStatus.Scheduled)
                return Result.Failure(DomainErrors.Appointment.OnlyScheduledCanBeRescheduled);

            if (newStartDate < nowUtc)
                return Result.Failure(DomainErrors.Appointment.InThePast);

            var intervalResult = TimeInterval.Create(newStartDate, newEndDate);
            if (intervalResult.IsFailure)
                return Result.Failure(intervalResult.Error);

            TimeInterval = intervalResult.Value;

            RaiseDomainEvent(new AppointmentRescheduledEvent(Id, PatientId, TimeInterval.Start, TimeInterval.End, nowUtc));

            return Result.Success();
        }

        public Result MarkConfirmationSent(DateTime nowUtc)
        {
            if (ConfirmationSentAtUtc is not null)
                return Result.Failure(DomainErrors.Appointment.ConfirmationAlreadySent);

            ConfirmationSentAtUtc = nowUtc;
            return Result.Success();
        }
    }

}
