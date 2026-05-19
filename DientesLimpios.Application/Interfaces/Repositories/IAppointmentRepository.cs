using DientesLimpios.Application.Interfaces.Repositories.Models;
using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.Interfaces.Repositories
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<bool> AppointmentOverlaps(Guid dentistId, DateTime start, DateTime end, CancellationToken cancellationToken = default);

        // Obtiene una appointment por su Id, incluyendo las entidades relacionadas y reemplaza el método base
        Task<Appointment?> GetById(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Appointment>> GetFiltered(AppointmentFilterDTO appointmentFilterDTO, CancellationToken cancellationToken = default);
    }
}
