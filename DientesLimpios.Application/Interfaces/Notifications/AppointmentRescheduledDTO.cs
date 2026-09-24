namespace DientesLimpios.Application.Interfaces.Notifications
{
    public class AppointmentRescheduledDTO
    {
        public required Guid Id { get; set; }
        public required string Patient { get; set; }
        public required string PatientEmail { get; set; }
        public required DateTime NewStartDate { get; set; }
        public required DateTime NewEndDate { get; set; }
    }
}
