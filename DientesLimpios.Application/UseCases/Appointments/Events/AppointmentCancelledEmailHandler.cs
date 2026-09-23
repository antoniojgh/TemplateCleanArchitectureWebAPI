using DientesLimpios.Application.Interfaces.Notifications;
using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Appointments.Events
{

    public sealed class AppointmentCancelledEmailHandler(
    IApplicationDbContext db,
    INotificationService notificationService,
    TimeProvider timeProvider,
    ILogger<AppointmentCancelledEmailHandler> logger) : IDomainEventHandler<AppointmentCancelledEvent>
    {
        // No catch-all: a failure has to reach OutboxProcessor, which records it and retries.
        public async Task Handle(AppointmentCancelledEvent domainEvent, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);

            var data = await (
                from a in db.Appointments.Where(a => a.Id == domainEvent.AppointmentId)
                join p in db.Patients on a.PatientId equals p.Id
                select new
                {
                    a.CancellationSentAtUtc,
                    Dto = new AppointmentCancellationDTO
                    {
                        Id = a.Id,
                        Date = a.TimeInterval.Start,
                        Patient = p.Name,
                        PatientEmail = p.Email.Value
                    }
                }).FirstOrDefaultAsync(cancellationToken);

            if (data is null)
            {
                // Stored in the same transaction as this event, so it can only be missing if it
                // was deleted afterwards. A retry would not change that.
                logger.LogWarning(
                    "Appointment {AppointmentId} not found while handling AppointmentCancelledEvent.",
                    domainEvent.AppointmentId);
                return;
            }

            // The outbox delivers at least once, so this check has to come before the send.
            if (data.CancellationSentAtUtc is not null)
            {
                logger.LogInformation(
                    "Cancellation notice for appointment {AppointmentId} was already sent; skipping.",
                    domainEvent.AppointmentId);
                return;
            }

            var sentAtUtc = timeProvider.GetUtcNow().UtcDateTime;

            await notificationService.SendAppointmentCancellation(data.Dto, cancellationToken);

            // Delivery marker, not state-machine data: written directly so the concurrency token
            // cannot veto it. The WHERE guard keeps it idempotent when two processors race.
            await db.Appointments
                    .Where(a => a.Id == domainEvent.AppointmentId && a.CancellationSentAtUtc == null)
                    .ExecuteUpdateAsync(
                        s => s.SetProperty(a => a.CancellationSentAtUtc, sentAtUtc),
                        cancellationToken);

            logger.LogInformation(
                "Cancellation confirmation email sent to {Email} for appointment {AppointmentId}.",
                data.Dto.PatientEmail, data.Dto.Id);
        }
    }

}