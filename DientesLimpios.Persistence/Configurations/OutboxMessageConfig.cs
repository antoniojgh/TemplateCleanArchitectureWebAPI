using DientesLimpios.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DientesLimpios.Persistence.Configurations
{
    public class OutboxMessageConfig : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");
            builder.Property(x => x.Type).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Payload).IsRequired();
            builder.Property(x => x.Error).HasMaxLength(OutboxMessage.ErrorMaxLength);

            // The processor only reads unprocessed rows, oldest first. A filtered index keeps
            // that query cheap no matter how many processed rows pile up.
            builder.HasIndex(x => x.OccurredOnUtc).HasFilter("[ProcessedOnUtc] IS NULL");
        }
    }
}
