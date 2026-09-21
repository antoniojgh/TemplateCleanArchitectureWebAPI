using DientesLimpios.Application.Interfaces.Notifications;
using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Application.UseCases.Appointments.Commands.CreateAppointment;
using DientesLimpios.Application.Utilities.Mediator;
using DientesLimpios.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DientesLimpios.Application.UseCases.Appointments.Events
{
    public sealed class AppointmentCreatedEmailHandler(
        IAppointmentRepository repository,
        IApplicationDbContext db,
        INotificationService notificationService,
        TimeProvider timeProvider,
        ILogger<AppointmentCreatedEmailHandler> logger) : IDomainEventHandler<AppointmentCreatedEvent>
    {
        // No catch-all: a failure has to reach OutboxProcessor, which records it and retries.
        public async Task Handle(AppointmentCreatedEvent domainEvent, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(domainEvent);

            var appointment = await repository.GetById(domainEvent.AppointmentId, cancellationToken);

            if (appointment is null)
            {
                // Stored in the same transaction as this event, so it can only be missing if it
                // was deleted afterwards. A retry would not change that.
                logger.LogWarning(
                    "Appointment {AppointmentId} not found while handling AppointmentCreatedEvent.",
                    domainEvent.AppointmentId);
                return;
            }

            // The outbox delivers at least once, so this check has to come before the send.
            if (appointment.ConfirmationSentAtUtc is not null)
            {
                logger.LogInformation(
                    "Confirmation for appointment {AppointmentId} was already sent; skipping.",
                    appointment.Id);
                return;
            }

            var dto = appointment.ADto();
            var sentAtUtc = timeProvider.GetUtcNow().UtcDateTime;

            await notificationService.SendAppointmentConfirmation(dto, cancellationToken);

            // The email has been sent, so the marker must land even if the appointment changed
            // meanwhile. This writes the one column directly: it is a delivery marker, not part
            // of the state machine that Cancel/Complete guard, so the concurrency token must not
            // veto it. The WHERE guard keeps it idempotent when two processors race.
            await db.Appointments
                    .Where(a => a.Id == appointment.Id && a.ConfirmationSentAtUtc == null)
                    .ExecuteUpdateAsync(
                        s => s.SetProperty(a => a.ConfirmationSentAtUtc, sentAtUtc),
                        cancellationToken);

            logger.LogInformation(
                "Confirmation email sent to {Email} for appointment {AppointmentId}.",
                dto.PatientEmail, dto.Id);
        }
    }
}