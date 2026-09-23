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

        private readonly ConcurrentBag<AppointmentCancellationDTO> _cancelledConfirmations = [];

        // Keyed by appointment: tests share one database, so a failure that was not tied to
        // a specific appointment could be consumed by a message another test left behind.
        // One set per email type: the same appointment raises both a confirmation and a
        // cancellation, and a shared set would let whichever is sent first consume the failure.
        private readonly ConcurrentDictionary<Guid, byte> _failConfirmationOnce = new();
        private readonly ConcurrentDictionary<Guid, byte> _failCancellationOnce = new();

        public IReadOnlyCollection<AppointmentConfirmationDTO> Confirmations => _confirmations;

        public IReadOnlyCollection<AppointmentCancellationDTO> CancelledConfirmations => _cancelledConfirmations;

        public void FailNextConfirmationFor(Guid appointmentId) => _failConfirmationOnce.TryAdd(appointmentId, 0);

        public void FailNextCancellationFor(Guid appointmentId) => _failCancellationOnce.TryAdd(appointmentId, 0);

        // Lets a test simulate someone editing the appointment while the email is in flight.
        private Func<AppointmentConfirmationDTO, Task>? _beforeConfirmation;
        private Func<AppointmentCancellationDTO, Task>? _beforeCancellation;
        public void BeforeNextConfirmation(Func<AppointmentConfirmationDTO, Task> hook) =>
            Interlocked.Exchange(ref _beforeConfirmation, hook);
        public void BeforeNextCancellation(Func<AppointmentCancellationDTO, Task> hook) =>
            Interlocked.Exchange(ref _beforeCancellation, hook);

        public async Task SendAppointmentConfirmation(AppointmentConfirmationDTO appointment, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(appointment);

            // Removing the entry means the next attempt for this appointment succeeds.
            if (_failConfirmationOnce.TryRemove(appointment.Id, out _))
                throw new InvalidOperationException("Simulated SMTP failure.");

            var hook = Interlocked.Exchange(ref _beforeConfirmation, null);
            if (hook is not null)
                await hook(appointment);

            _confirmations.Add(appointment);
        }

        public async Task SendAppointmentCancellation(AppointmentCancellationDTO appointment, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(appointment);

            // Removing the entry means the next attempt for this appointment succeeds.
            if (_failCancellationOnce.TryRemove(appointment.Id, out _))
                throw new InvalidOperationException("Simulated SMTP failure.");

            var hook = Interlocked.Exchange(ref _beforeCancellation, null);
            if (hook is not null)
                await hook(appointment);

            _cancelledConfirmations.Add(appointment);
        }

        public Task SendAppointmentReminder(AppointmentReminderDTO appointment, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}