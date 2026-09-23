namespace DientesLimpios.Application.Interfaces.Notifications
{
    public class AppointmentCancellationDTO
    {
        public required Guid Id { get; set; }
        public required string Patient { get; set; }
        public required string PatientEmail { get; set; }
        public required DateTime Date { get; set; }
    }
}
