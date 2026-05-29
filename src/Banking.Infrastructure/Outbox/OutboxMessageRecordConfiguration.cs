using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Outbox;

internal sealed class OutboxMessageRecordConfiguration : IEntityTypeConfiguration<OutboxMessageRecord>
{
    public void Configure(EntityTypeBuilder<OutboxMessageRecord> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.Id).HasColumnName("id").IsRequired();
        builder.Property(message => message.Type).HasColumnName("type").HasMaxLength(512).IsRequired();
        builder.Property(message => message.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(message => message.Headers).HasColumnName("headers").HasColumnType("jsonb").IsRequired();
        builder.Property(message => message.OccurredOn).HasColumnName("occurred_on").IsRequired();
        builder.Property(message => message.ProcessedOn).HasColumnName("processed_on");
        builder.Property(message => message.RetryCount).HasColumnName("retry_count").IsRequired();
        builder.Property(message => message.Status).HasColumnName("status").HasMaxLength(32).IsRequired();
        builder.Property(message => message.Error).HasColumnName("error").HasMaxLength(4000);
        builder.Property(message => message.CorrelationId).HasColumnName("correlation_id").IsRequired();

        builder.HasIndex(message => new { message.Status, message.OccurredOn })
            .HasDatabaseName("ix_outbox_messages_status_occurred_on");
    }
}

