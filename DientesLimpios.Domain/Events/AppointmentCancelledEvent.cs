using DientesLimpios.Domain.Common;

namespace DientesLimpios.Domain.Events
{
    // OccurredOnUtc is passed in by the aggregate, which received it from the caller:
    // Domain never reads the clock.
    public sealed record AppointmentCancelledEvent(
        Guid AppointmentId,
        Guid PatientId,
        DateTime StartDate,
        DateTime OccurredOnUtc) : IDomainEvent
    {
        public Guid EventId { get; init; } = Guid.CreateVersion7();
    }
}
