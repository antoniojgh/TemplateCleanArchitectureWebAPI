using DientesLimpios.Application.UseCases.Patients.Queries.GetPatientList;
using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.Interfaces.Repositories
{
    public interface IPatientRepository
    {
        Task<IEnumerable<Patient>> GetFiltered(PatientFilterDTO filter, CancellationToken cancellationToken = default);
    }
}
