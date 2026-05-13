using DientesLimpios.Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Identity
{
    public class DientesLimpiosIdentityDbContext : IdentityDbContext<User>
    {
        public DientesLimpiosIdentityDbContext(DbContextOptions<DientesLimpiosIdentityDbContext> options) :
            base(options)
        {
        }

        protected DientesLimpiosIdentityDbContext()
        {
        }
    }
}
