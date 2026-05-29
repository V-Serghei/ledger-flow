using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Pix;

internal sealed class PixKeyRecordConfiguration : IEntityTypeConfiguration<PixKeyRecord>
{
    public void Configure(EntityTypeBuilder<PixKeyRecord> builder)
    {
        builder.ToTable("pix_keys");

        builder.HasKey(key => key.Id);

        builder.Property(key => key.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(key => key.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(key => key.OwnerUserId)
            .HasColumnName("owner_user_id")
            .IsRequired();

        builder.Property(key => key.Type)
            .HasColumnName("type")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(key => key.Key)
            .HasColumnName("key")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(key => key.NormalizedKey)
            .HasColumnName("normalized_key")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(key => key.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(key => key.NormalizedKey)
            .IsUnique()
            .HasDatabaseName("ix_pix_keys_normalized_key");

        builder.HasIndex(key => key.AccountId)
            .HasDatabaseName("ix_pix_keys_account_id");

        builder.HasIndex(key => key.OwnerUserId)
            .HasDatabaseName("ix_pix_keys_owner_user_id");
    }
}

