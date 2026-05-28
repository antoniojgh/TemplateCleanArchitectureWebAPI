using DientesLimpios.Domain.Common;

namespace DientesLimpios.Domain.Events
{
    public sealed record AppointmentCreatedEvent(
        Guid AppointmentId,
        Guid PatientId,
        Guid DentistId,
        Guid OfficeId,
        DateTime StartDate,
        DateTime EndDate) : IDomainEvent
    {
        public Guid EventId { get; } = Guid.CreateVersion7();
        public DateTime OccurredOnUtc { get; } = DateTime.UtcNow;
    }

}
