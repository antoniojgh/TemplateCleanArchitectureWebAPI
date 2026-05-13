using DientesLimpios.Application.Interfaces.Repositories.Models;
using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.Interfaces.Repositories
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<bool> AppointmentOverlaps(Guid dentistId, DateTime start, DateTime end);

        // Obtiene una appointment por su Id, incluyendo las entidades relacionadas y reemplaza el método base
        new Task<Appointment?> GetById(Guid id);
        Task<IEnumerable<Appointment>> GetFiltered(AppointmentFilterDTO appointmentFilterDTO);
    }
}
