using DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Persistence.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Persistence.Repositories
{
    public sealed class DentistRepository(DientesLimpiosDbContext context) : IDentistRepository
    {
        public async Task<IEnumerable<Dentist>> GetFiltered(DentistFilterDTO filter, CancellationToken cancellationToken = default)
        {
            var queryable = context.Dentists.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                queryable = queryable.Where(x => x.Name.Contains(filter.Name));
            }

            if (!string.IsNullOrWhiteSpace(filter.Email))
            {
                queryable = queryable.Where(x => x.Email.Value.Contains(filter.Email));
            }

            return await queryable.OrderBy(x => x.Name).Paginar(filter.Pagina, filter.RegistrosPorPagina).ToListAsync(cancellationToken);
        }
    }
}
