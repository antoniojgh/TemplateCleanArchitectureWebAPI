using DientesLimpios.Application.UseCases.Patients.Queries.GetPatientList;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Persistence.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Persistence.Repositories
{
    public sealed class PatientRepository(DientesLimpiosDbContext context) : IPatientRepository
    {
        public async Task<(IEnumerable<Patient> patients, int totalCount)> GetFiltered(PatientFilterDTO filter, CancellationToken cancellationToken = default)
        {
            var queryable = context.Patients.AsNoTracking();

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
            var patients = await queryable.OrderBy(x => x.Name)
                .Paginar(filter.Page, filter.RecordsPerPage).ToListAsync(cancellationToken);

            return (patients, totalCount);
        }
    }
}
