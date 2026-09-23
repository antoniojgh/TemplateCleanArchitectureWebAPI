namespace DientesLimpios.Application.Interfaces.Notifications
{
    public interface INotificationService
    {
        Task SendAppointmentConfirmation(AppointmentConfirmationDTO appointment, CancellationToken cancellationToken);

        Task SendAppointmentCancellation(AppointmentCancellationDTO appointment, CancellationToken cancellationToken);

        Task SendAppointmentReminder(AppointmentReminderDTO appointment, CancellationToken cancellationToken);
    }
}
