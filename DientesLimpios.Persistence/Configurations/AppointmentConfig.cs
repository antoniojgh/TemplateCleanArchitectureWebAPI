using DientesLimpios.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DientesLimpios.Persistence.Configurations
{
    public class AppointmentConfig : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.ComplexProperty(prop => prop.TimeInterval, action =>
            {
                action.Property(e => e.Start).HasColumnName("StartDate");
                action.Property(e => e.End).HasColumnName("EndDate");
            });

            // Appointments are medical and financial history: deleting a dentist, patient or
            // office must never destroy them. Restrict makes the database refuse — the backstop
            // behind the explicit rule in the delete handlers, which is what produces the 409.
            builder.HasOne<Patient>()
                   .WithMany()
                   .HasForeignKey(a => a.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Dentist>()
                   .WithMany()
                   .HasForeignKey(a => a.DentistId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Office>()
                   .WithMany()
                   .HasForeignKey(a => a.OfficeId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
