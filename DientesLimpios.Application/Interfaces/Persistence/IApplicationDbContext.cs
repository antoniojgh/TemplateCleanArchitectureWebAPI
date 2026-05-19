using DientesLimpios.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DientesLimpios.Application.Interfaces.Persistence
{
    public interface IApplicationDbContext
    {
        DbSet<Appointment> Appointments { get; }
        DbSet<Patient> Patients { get; }
        DbSet<Dentist> Dentists { get; }
        DbSet<Office> Offices { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
