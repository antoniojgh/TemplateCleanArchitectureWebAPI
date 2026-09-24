using DientesLimpios.Application.Interfaces.Notifications;
using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Appointments.Events
{

    public sealed class AppointmentRescheduledEmailHandler(
    IApplicationDbContext db,
    INotificationService notificationService,
    ILogger<AppointmentRescheduledEmailHandler> logger) : IDomainEventHandler<AppointmentRescheduledEvent>
    {
        // No catch-all: a failure has to reach OutboxProcessor, which records it and retries.
        public async Task Handle(AppointmentRescheduledEvent domainEvent, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);

            var data = await (
                from a in db.Appointments.Where(a => a.Id == domainEvent.AppointmentId)
                join p in db.Patients on a.PatientId equals p.Id
                select new
                {
                    a.RescheduleNoticeEventId,
                    Dto = new AppointmentRescheduledDTO
                    {
                        Id = a.Id,
                        NewStartDate = a.TimeInterval.Start,
                        NewEndDate = a.TimeInterval.End,
                        Patient = p.Name,
                        PatientEmail = p.Email.Value
                    }
                }).FirstOrDefaultAsync(cancellationToken);

            if (data is null)
            {
                // Stored in the same transaction as this event, so it can only be missing if it
                // was deleted afterwards. A retry would not change that.
                logger.LogWarning(
                    "Appointment {AppointmentId} not found while handling AppointmentRescheduledEvent.",
                    domainEvent.AppointmentId);
                return;
            }

            // The outbox delivers at least once, so this check has to come before the send.
            // The key is the event, not the appointment: a later reschedule of the same
            // appointment is a new event and gets its own email.
            if (data.RescheduleNoticeEventId == domainEvent.EventId)
            {
                logger.LogInformation(
                    "Reschedule notice for event {EventId} on appointment {AppointmentId} was already sent; skipping.",
                    domainEvent.EventId, domainEvent.AppointmentId);
                return;
            }

            await notificationService.SendAppointmentRescheduled(data.Dto, cancellationToken);

            // Delivery marker, not state-machine data: written directly so the concurrency token
            // cannot veto it. It has its own column, so it never touches ConfirmationSentAtUtc,
            // which belongs to the booking email.
            await db.Appointments
                    .Where(a => a.Id == domainEvent.AppointmentId)
                    .ExecuteUpdateAsync(
                        s => s.SetProperty(a => a.RescheduleNoticeEventId, domainEvent.EventId),
                        cancellationToken);

            logger.LogInformation(
                "Rescheduled confirmation email sent to {Email} for appointment {AppointmentId}.",
                data.Dto.PatientEmail, data.Dto.Id);
        }
    }

}