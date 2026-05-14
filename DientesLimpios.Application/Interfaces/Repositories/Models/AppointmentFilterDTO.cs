using DientesLimpios.Domain.Enums;

namespace DientesLimpios.Application.Interfaces.Repositories.Models
{
    public class AppointmentFilterDTO
    {
        public Guid? PatientId { get; set; }
        public Guid? DentistId { get; set; }
        public Guid? OfficeId { get; set; }
        public AppointmentStatus? AppointmentStatus { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
