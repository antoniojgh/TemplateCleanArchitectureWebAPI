using DientesLimpios.Application.Interfaces.Notifications;
using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Appointments.Events
{

    public sealed class AppointmentCreatedEmailHandler(
    IApplicationDbContext db,
    INotificationService notificationService,
    TimeProvider timeProvider,
    ILogger<AppointmentCreatedEmailHandler> logger) : IDomainEventHandler<AppointmentCreatedEvent>
    {
        // No catch-all: a failure has to reach OutboxProcessor, which records it and retries.
        public async Task Handle(AppointmentCreatedEvent domainEvent, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);

            var data = await (
                from a in db.Appointments.Where(a => a.Id == domainEvent.AppointmentId)
                join p in db.Patients on a.PatientId equals p.Id
                join d in db.Dentists on a.DentistId equals d.Id
                join o in db.Offices on a.OfficeId equals o.Id
                select new
                {
                    a.ConfirmationSentAtUtc,
                    Dto = new AppointmentConfirmationDTO
                    {
                        Id = a.Id,
                        Date = a.TimeInterval.Start,
                        Patient = p.Name,
                        PatientEmail = p.Email.Value,
                        Dentist = d.Name,
                        Office = o.Name
                    }
                }).FirstOrDefaultAsync(cancellationToken);

            if (data is null)
            {
                // Stored in the same transaction as this event, so it can only be missing if it
                // was deleted afterwards. A retry would not change that.
                logger.LogWarning(
                    "Appointment {AppointmentId} not found while handling AppointmentCreatedEvent.",
                    domainEvent.AppointmentId);
                return;
            }


            // The outbox delivers at least once, so this check has to come before the send.
            if (data.ConfirmationSentAtUtc is not null)
            {
                logger.LogInformation(
                    "Confirmation for appointment {AppointmentId} was already sent; skipping.",
                    domainEvent.AppointmentId);
                return;
            }

            var sentAtUtc = timeProvider.GetUtcNow().UtcDateTime;

            await notificationService.SendAppointmentConfirmation(data.Dto, cancellationToken);

            // The email has been sent, so the marker must land even if the appointment changed
            // meanwhile. This writes the one column directly: it is a delivery marker, not part
            // of the state machine that Cancel/Complete guard, so the concurrency token must not
            // veto it. The WHERE guard keeps it idempotent when two processors race.
            await db.Appointments
                    .Where(a => a.Id == domainEvent.AppointmentId && a.ConfirmationSentAtUtc == null)
                    .ExecuteUpdateAsync(
                        s => s.SetProperty(a => a.ConfirmationSentAtUtc, sentAtUtc),
                        cancellationToken);

            logger.LogInformation(
                "Confirmation email sent to {Email} for appointment {AppointmentId}.",
                data.Dto.PatientEmail, data.Dto.Id);
        }
    }

}