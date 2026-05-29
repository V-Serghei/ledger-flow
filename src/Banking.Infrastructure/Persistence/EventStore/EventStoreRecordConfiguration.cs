using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Persistence.EventStore;

internal sealed class EventStoreRecordConfiguration : IEntityTypeConfiguration<EventStoreRecord>
{
    public void Configure(EntityTypeBuilder<EventStoreRecord> builder)
    {
        builder.ToTable("event_store");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(record => record.AggregateId)
            .HasColumnName("aggregate_id")
            .IsRequired();

        builder.Property(record => record.AggregateType)
            .HasColumnName("aggregate_type")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(record => record.EventType)
            .HasColumnName("event_type")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(record => record.EventVersion)
            .HasColumnName("event_version")
            .IsRequired();

        builder.Property(record => record.AggregateVersion)
            .HasColumnName("aggregate_version")
            .IsRequired();

        builder.Property(record => record.Payload)
            .HasColumnName("payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(record => record.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(record => record.OccurredOn)
            .HasColumnName("occurred_on")
            .IsRequired();

        builder.Property(record => record.CorrelationId)
            .HasColumnName("correlation_id")
            .IsRequired();

        builder.Property(record => record.CausationId)
            .HasColumnName("causation_id");

        builder.Property(record => record.UserId)
            .HasColumnName("user_id");

        builder.HasIndex(record => record.AggregateId)
            .HasDatabaseName("ix_event_store_aggregate_id");

        builder.HasIndex(record => record.OccurredOn)
            .HasDatabaseName("ix_event_store_occurred_on");

        builder.HasIndex(record => new { record.AggregateId, record.AggregateVersion })
            .HasDatabaseName("ix_event_store_aggregate_id_aggregate_version")
            .IsUnique();
    }
}
