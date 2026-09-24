namespace DientesLimpios.API.DTOs.Appointments
{
    public class RescheduleAppointmentDTO
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
