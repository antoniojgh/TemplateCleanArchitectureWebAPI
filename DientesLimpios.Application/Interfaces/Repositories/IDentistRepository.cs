using DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList;
using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.Interfaces.Repositories
{
    public interface IDentistRepository 
    {
        Task<(IEnumerable<Dentist> dentists, int totalCount)> GetFiltered(DentistFilterDTO filter, CancellationToken cancellationToken = default);
    }
}
