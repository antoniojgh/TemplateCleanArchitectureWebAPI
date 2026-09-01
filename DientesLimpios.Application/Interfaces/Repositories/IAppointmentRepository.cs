using DientesLimpios.Application.Interfaces.Repositories.Models;
using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.Interfaces.Repositories
{
    public interface IAppointmentRepository 
    {
        Task<bool> AppointmentOverlaps(Guid dentistId, DateTime start, DateTime end, CancellationToken cancellationToken = default);

        // Gets an appointment by its ID, including related entities, and replaces the base method
        Task<Appointment?> GetById(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Appointment>> GetFiltered(AppointmentFilterDTO appointmentFilterDTO, CancellationToken cancellationToken = default);
    }
}
