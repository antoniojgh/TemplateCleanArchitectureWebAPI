using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Persistence
{
    public class DientesLimpiosDbContext : DbContext, IApplicationDbContext
    {
        public DientesLimpiosDbContext(DbContextOptions<DientesLimpiosDbContext> options) : base(options)
        {
        }
        public DientesLimpiosDbContext()
        {
        }
        public DbSet<Office> Offices { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Dentist> Dentists { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Applies all entity configurations in the current assembly
            // i.e., the configurations located in the "Configurations" folder
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DientesLimpiosDbContext).Assembly);
        }


    }
}
