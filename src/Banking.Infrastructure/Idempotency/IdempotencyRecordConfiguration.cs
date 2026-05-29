using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Idempotency;

internal sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_records");

        builder.HasKey(record => record.Id);

        builder.Property(record => record.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(record => record.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(record => record.OperationType)
            .HasColumnName("operation_type")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(record => record.IdempotencyKey)
            .HasColumnName("idempotency_key")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(record => record.RequestHash)
            .HasColumnName("request_hash")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(record => record.ResponsePayload)
            .HasColumnName("response_payload")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(record => record.StatusCode)
            .HasColumnName("status_code")
            .IsRequired();

        builder.Property(record => record.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(record => record.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.HasIndex(record => new { record.UserId, record.OperationType, record.IdempotencyKey })
            .IsUnique()
            .HasDatabaseName("ix_idempotency_records_user_operation_key");
    }
}

