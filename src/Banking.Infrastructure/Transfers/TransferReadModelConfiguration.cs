using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Transfers;

internal sealed class TransferReadModelConfiguration : IEntityTypeConfiguration<TransferReadModel>
{
    public void Configure(EntityTypeBuilder<TransferReadModel> builder)
    {
        builder.ToTable("transfers_read_model");

        builder.HasKey(transfer => transfer.Id);

        builder.Property(transfer => transfer.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(transfer => transfer.SourceAccountId)
            .HasColumnName("source_account_id")
            .IsRequired();

        builder.Property(transfer => transfer.DestinationAccountId)
            .HasColumnName("destination_account_id")
            .IsRequired();

        builder.Property(transfer => transfer.OwnerUserId)
            .HasColumnName("owner_user_id")
            .IsRequired();

        builder.Property(transfer => transfer.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(transfer => transfer.Status)
            .HasColumnName("status")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(transfer => transfer.Description)
            .HasColumnName("description")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(transfer => transfer.OccurredOn)
            .HasColumnName("occurred_on")
            .IsRequired();

        builder.HasIndex(transfer => transfer.SourceAccountId)
            .HasDatabaseName("ix_transfers_read_model_source_account_id");

        builder.HasIndex(transfer => transfer.DestinationAccountId)
            .HasDatabaseName("ix_transfers_read_model_destination_account_id");

        builder.HasIndex(transfer => transfer.OwnerUserId)
            .HasDatabaseName("ix_transfers_read_model_owner_user_id");
    }
}

