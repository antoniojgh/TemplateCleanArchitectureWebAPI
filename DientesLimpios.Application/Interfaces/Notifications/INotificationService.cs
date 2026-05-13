namespace DientesLimpios.Application.Interfaces.Notifications
{
    public interface INotificationService
    {
        Task SendAppointmentConfirmation(AppointmentConfirmationDTO appointment);
        Task SendAppointmentReminder(AppointmentReminderDTO appointment);
    }
}
