using DientesLimpios.Aplicacion.Interfaces.Identidad;
using DientesLimpios.Dominio.Comunes;
using DientesLimpios.Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Persistencia
{
    public class DientesLimpiosDbContext : DbContext
    {
        private readonly IServicioUsuarios? _servicioUsuarios;
        public DientesLimpiosDbContext(DbContextOptions<DientesLimpiosDbContext> options, IServicioUsuarios servicioUsuarios) : base(options)
        {
            _servicioUsuarios = servicioUsuarios;
        }
        public DientesLimpiosDbContext()
        {
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            if (_servicioUsuarios is not null)
            {
                // Recorre las entidades que están siendo agregadas o modificadas y heredan de EntidadAuditable
                foreach (var entry in ChangeTracker.Entries<EntidadAuditable>())
                {
                    // Actualiza los campos de auditoría según el estado de la entidad
                    switch (entry.State)
                    {
                        // Si la entidad está siendo agregada
                        case EntityState.Added:
                            entry.Entity.FechaCreacion = DateTime.UtcNow;
                            entry.Entity.CreadoPor = _servicioUsuarios.ObtenerUsuarioId();
                            break;
                        // Si la entidad está siendo modificada
                        case EntityState.Modified:
                            entry.Entity.UltimaFechaModificacion = DateTime.UtcNow;
                            entry.Entity.UltimaModificacionPor = _servicioUsuarios.ObtenerUsuarioId();
                            break;
                    }
                }
            }


            return base.SaveChangesAsync(cancellationToken);
        }

        public DbSet<Consultorio> Consultorios { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Dentista> Dentistas { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public IServicioUsuarios ServicioUsuarios { get; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplica todas las configuraciones de entidades en el ensamblado actual
            // es decir, las configuraciones que están en la carpeta "Configuraciones"
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DientesLimpiosDbContext).Assembly);
        }


    }
}
