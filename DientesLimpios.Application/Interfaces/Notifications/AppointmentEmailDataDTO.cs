namespace DientesLimpios.Application.Interfaces.Notifications
{
    public class AppointmentEmailDataDTO
    {
        public required Guid Id { get; set; }
        public required string Patient { get; set; }
        public required string PatientEmail { get; set; }
        public required string Dentist { get; set; }
        public required string Office { get; set; }
        public required DateTime Date { get; set; }
    }
}
