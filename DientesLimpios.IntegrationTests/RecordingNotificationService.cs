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

        public IReadOnlyCollection<AppointmentConfirmationDTO> Confirmations => _confirmations;

        public Task SendAppointmentConfirmation(AppointmentConfirmationDTO appointment)
        {
            _confirmations.Add(appointment);
            return Task.CompletedTask;
        }

        public Task SendAppointmentReminder(AppointmentReminderDTO appointment) => Task.CompletedTask;
    }
}
