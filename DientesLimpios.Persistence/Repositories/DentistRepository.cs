using DientesLimpios.Application.UseCases.Dentists.Queries.GetDentistList;
using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Persistence.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Persistence.Repositories
{
    public class DentistRepository : Repository<Dentist>, IDentistRepository
    {
        private readonly DientesLimpiosDbContext context;

        public DentistRepository(DientesLimpiosDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<Dentist>> GetFiltered(DentistFilterDTO filter)
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

            return await queryable.OrderBy(x => x.Name).Paginar(filter.Pagina, filter.RegistrosPorPagina).ToListAsync();
        }
    }
}
