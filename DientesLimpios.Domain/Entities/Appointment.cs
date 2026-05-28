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
        public Patient? Patient { get; private set; }
        public Dentist? Dentist { get; private set; }
        public Office? Office { get; private set; }

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
                appointment.Id, patientId, dentistId, officeId, startDate, endDate));


            return Result.Success(appointment);
        }

        public Result Cancel()
        {
            if (Status != AppointmentStatus.Scheduled)
                return Result.Failure(DomainErrors.Appointment.OnlyScheduledCanBeCancelled);

            Status = AppointmentStatus.Cancelled;
            return Result.Success();
        }

        public Result Complete()
        {
            if (Status != AppointmentStatus.Scheduled)
                return Result.Failure(DomainErrors.Appointment.OnlyScheduledCanBeCompleted);

            Status = AppointmentStatus.Completed;
            return Result.Success();
        }
    }

}
