using DientesLimpios.Application.Interfaces.Identity;
using DientesLimpios.Domain.Common;
using DientesLimpios.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Persistence
{
    public class DientesLimpiosDbContext : DbContext
    {
        private readonly IUserService? _userService;
        public DientesLimpiosDbContext(DbContextOptions<DientesLimpiosDbContext> options, IUserService userService) : base(options)
        {
            _userService = userService;
        }
        public DientesLimpiosDbContext()
        {
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_userService is not null)
            {
                // Recorre las entidades que están siendo agregadas o modificadas y heredan de AuditableEntity
                foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
                {
                    // Actualiza los campos de auditoría según el status de la entidad
                    switch (entry.State)
                    {
                        // Si la entidad está siendo agregada
                        case EntityState.Added:
                            entry.Entity.CreatedDate = DateTime.UtcNow;
                            entry.Entity.CreatedBy = _userService.GetUserId();
                            break;
                        // Si la entidad está siendo modificada
                        case EntityState.Modified:
                            entry.Entity.LastModifiedDate = DateTime.UtcNow;
                            entry.Entity.LastModifiedBy = _userService.GetUserId();
                            break;
                    }
                }
            }


            return base.SaveChangesAsync(cancellationToken);
        }

        public DbSet<Office> Offices { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Dentist> Dentists { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public IUserService? UserService { get; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplica todas las configuraciones de entidades en el ensamblado actual
            // es decir, las configuraciones que están en la carpeta "Configurations"
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DientesLimpiosDbContext).Assembly);
        }


    }
}
