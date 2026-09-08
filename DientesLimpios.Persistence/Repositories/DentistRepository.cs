using DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Persistence.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Persistence.Repositories
{
    public sealed class DentistRepository(DientesLimpiosDbContext context) : IDentistRepository
    {
        public async Task<(IEnumerable<Dentist> dentists, int totalCount)> GetFiltered(DentistFilterDTO filter, CancellationToken cancellationToken = default)
        {
            var queryable = context.Dentists.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                queryable = queryable.Where(x => x.Name.Contains(filter.Name));
            }

            if (!string.IsNullOrWhiteSpace(filter.Email))
            {
                queryable = queryable.Where(x => x.Email.Value.Contains(filter.Email));
            }

            // 1. Get the total count of the filtered data BEFORE pagination
            var totalCount = await queryable.CountAsync(cancellationToken);

            // 2. Apply pagination and fetch the specific page of data
            var dentists = await queryable.OrderBy(x => x.Name).Paginar(filter.Page, filter.RecordsPerPage).ToListAsync(cancellationToken);

            return (dentists, totalCount);
        }
    }
}
