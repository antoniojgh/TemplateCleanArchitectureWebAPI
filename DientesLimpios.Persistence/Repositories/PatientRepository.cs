using DientesLimpios.Application.UseCases.Patients.Queries.GetPatientList;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Persistence.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Persistence.Repositories
{
    public class PatientRepository : Repository<Patient>, IPatientRepository
    {
        private readonly DientesLimpiosDbContext context;

        public PatientRepository(DientesLimpiosDbContext context)
            : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<Patient>> GetFiltered(PatientFilterDTO filter, CancellationToken cancellationToken = default)
        {
            var queryable = context.Patients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                queryable = queryable.Where(x => x.Name.Contains(filter.Name));
            }

            if (!string.IsNullOrWhiteSpace(filter.Email))
            {
                queryable = queryable.Where(x => x.Email.Value.Contains(filter.Email));
            }


            return await queryable.OrderBy(x => x.Name)
                .Paginar(filter.Pagina, filter.RegistrosPorPagina).ToListAsync(cancellationToken);
        }
    }
}
