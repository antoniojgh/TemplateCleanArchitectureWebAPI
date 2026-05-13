using DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList;
using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Application.Interfaces.Repositories
{
    public interface IDentistRepository : IRepository<Dentist>
    {
        Task<IEnumerable<Dentist>> GetFiltered(DentistFilterDTO filter);
    }
}
