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

        // Lets a test simulate someone editing the appointment while the email is in flight.
        private Func<AppointmentConfirmationDTO, Task>? _beforeConfirmation;

        public void BeforeNextConfirmation(Func<AppointmentConfirmationDTO, Task> hook) =>
            Interlocked.Exchange(ref _beforeConfirmation, hook);

        public async Task SendAppointmentConfirmation(AppointmentConfirmationDTO appointment, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(appointment);

            // Removing the entry means the next attempt for this appointment succeeds.
            if (_failOnce.TryRemove(appointment.Id, out _))
                throw new InvalidOperationException("Simulated SMTP failure.");

            var hook = Interlocked.Exchange(ref _beforeConfirmation, null);
            if (hook is not null)
                await hook(appointment);

            _confirmations.Add(appointment);
        }

        public Task SendAppointmentReminder(AppointmentReminderDTO appointment, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}