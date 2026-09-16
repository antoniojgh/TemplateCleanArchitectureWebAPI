using System.Collections.Concurrent;
using DientesLimpios.Application.Interfaces.Notifications;

namespace DientesLimpios.IntegrationTests
{
    // Replaces EmailService for the duration of the test run. Without it the suite opens
    // a real SMTP connection to smtp.gmail.com for every appointment it creates, and it
    // could not observe whether a notification was raised at all.
    public sealed class RecordingNotificationService : INotificationService
    {
        private readonly ConcurrentBag<AppointmentConfirmationDTO> _confirmations = [];

        // Keyed by appointment: tests share one database, so a failure that was not tied to
        // a specific appointment could be consumed by a message another test left behind.
        private readonly ConcurrentDictionary<Guid, byte> _failOnce = new();

        public IReadOnlyCollection<AppointmentConfirmationDTO> Confirmations => _confirmations;

        public void FailNextConfirmationFor(Guid appointmentId) => _failOnce.TryAdd(appointmentId, 0);

        public Task SendAppointmentConfirmation(AppointmentConfirmationDTO appointment, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(appointment);

            // Removing the entry means the next attempt for this appointment succeeds.
            if (_failOnce.TryRemove(appointment.Id, out _))
                throw new InvalidOperationException("Simulated SMTP failure.");

            _confirmations.Add(appointment);
            return Task.CompletedTask;
        }

        public Task SendAppointmentReminder(AppointmentReminderDTO appointment, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}