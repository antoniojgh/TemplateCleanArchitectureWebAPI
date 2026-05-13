using DientesLimpios.Application.Interfaces.Repositories;
using DientesLimpios.Domain.Entities;

namespace DientesLimpios.Persistence.Repositories
{
    public class OfficeRepository : Repository<Office>, IOfficeRepository
    {
        public OfficeRepository(DientesLimpiosDbContext context) : base(context)
        {

        }
    }
}
