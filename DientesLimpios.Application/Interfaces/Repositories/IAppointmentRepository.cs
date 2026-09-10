using DientesLimpios.Application.Interfaces.Repositories.Models;
using DientesLimpios.Domain.Common.ResultPattern;
using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.Interfaces.Repositories
{
    public interface IAppointmentRepository 
    {
        // Checks the overlap rule and inserts atomically, under a lock keyed by dentist.
        // A separate "does it overlap?" query followed by an insert would be a
        // time-of-check/time-of-use race. createAppointment is invoked only once the slot
        // is confirmed free, so a rejected booking never constructs an aggregate or raises
        // its creation event.
        Task<Result<Guid>> AddIfNoOverlap(Guid patientId, Guid dentistId, Guid officeId, DateTime start, DateTime end, CancellationToken cancellationToken = default);

        // Gets an appointment by its ID, including related entities, and replaces the base method
        Task<Appointment?> GetById(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Appointment>> GetFiltered(AppointmentFilterDTO appointmentFilterDTO, CancellationToken cancellationToken = default);
    }
}
