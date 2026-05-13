namespace DientesLimpios.Application.UseCases.Appointments.Queries.GetAppointmentList
{
    public class AppointmentListDTO
    {
        public required Guid Id { get; set; }
        public required string Patient { get; set; }
        public required string Dentist { get; set; }
        public required string Office { get; set; }
        public required DateTime StartDate { get; set; }
        public required DateTime EndDate { get; set; }
        public required string AppointmentStatus { get; set; }
    }
}
