using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DientesLimpios.Persistence.Configurations
{
    public class PatientConfig : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.Property(prop => prop.Name)
            .HasMaxLength(Patient.NameMaxLength)
            .IsRequired();

            builder.ComplexProperty(prop => prop.Email, action =>
            {
                action.Property(e => e.Value).HasColumnName("Email").HasMaxLength(Email.MaxLength);
            });
        }
    }
}
