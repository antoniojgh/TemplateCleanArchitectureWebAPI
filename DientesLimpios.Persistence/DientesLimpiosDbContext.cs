using DientesLimpios.Application.Interfaces.Identity;
using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Domain.Common;
using DientesLimpios.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Persistence
{
    public class DientesLimpiosDbContext : DbContext, IApplicationDbContext
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
                // Iterates over entities being added or modified that inherit from AuditableEntity
                foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
                {
                    // Updates audit fields based on the entity state
                    switch (entry.State)
                    {
                        // If the entity is being added
                        case EntityState.Added:
                            entry.Entity.CreatedDate = DateTime.UtcNow;
                            entry.Entity.CreatedBy = _userService.GetUserId();
                            break;
                        // If the entity is being modified
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

            // Applies all entity configurations in the current assembly
            // i.e., the configurations located in the "Configurations" folder
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DientesLimpiosDbContext).Assembly);
        }


    }
}
