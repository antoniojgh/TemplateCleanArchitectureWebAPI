using DientesLimpios.Domain.Entities;
using DientesLimpios.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DientesLimpios.Persistence.Configurations
{
    public class DentistConfig : IEntityTypeConfiguration<Dentist>
    {
        public void Configure(EntityTypeBuilder<Dentist> builder)
        {
            builder.Property(prop => prop.Name)
            .HasMaxLength(Dentist.NameMaxLength)
            .IsRequired();

            builder.ComplexProperty(prop => prop.Email, action =>
            {
                action.Property(e => e.Value).HasColumnName("Email").HasMaxLength(Email.MaxLength);
            });
        }
    
    }
}
