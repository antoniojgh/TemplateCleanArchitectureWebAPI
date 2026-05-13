using DientesLimpios.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DientesLimpios.Persistence.Configurations
{
    public class OfficeConfig : IEntityTypeConfiguration<Office>
    {
        public void Configure(EntityTypeBuilder<Office> builder)
        {
            builder.Property(prop => prop.Name)
                .HasMaxLength(150)
                .IsRequired();
        }
    }
}
