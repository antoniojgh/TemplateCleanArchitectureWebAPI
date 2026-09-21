using DientesLimpios.Application.Interfaces.Persistence;
using DientesLimpios.Domain.Common;
using DientesLimpios.Domain.Entities;
using DientesLimpios.Persistence.Converters;
using DientesLimpios.Persistence.Outbox;
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
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Applies all entity configurations in the current assembly
            // i.e., the configurations located in the "Configurations" folder
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DientesLimpiosDbContext).Assembly);

            // Every aggregate gets an optimistic-concurrency token, so a second writer that
            // started from a stale copy is refused instead of overwriting the first.
            // OutboxMessage is deliberately excluded: the processor claims rows with locks.
            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                         .Where(t => typeof(AggregateRoot).IsAssignableFrom(t.ClrType)))
            {
                modelBuilder.Entity(entityType.ClrType)
                            .Property(nameof(AggregateRoot.RowVersion))
                            .IsRowVersion();
            }
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            ArgumentNullException.ThrowIfNull(configurationBuilder);

            // Every DateTime in this model is UTC (see Claude.md, "Time").
            configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
            configurationBuilder.Properties<DateTime?>().HaveConversion<UtcDateTimeConverter>();
        }


    }
}
