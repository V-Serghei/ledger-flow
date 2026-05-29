using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Accounts;

internal sealed class AccountReadModelConfiguration : IEntityTypeConfiguration<AccountReadModel>
{
    public void Configure(EntityTypeBuilder<AccountReadModel> builder)
    {
        builder.ToTable("accounts_read_model");

        builder.HasKey(account => account.Id);

        builder.Property(account => account.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(account => account.OwnerUserId)
            .HasColumnName("owner_user_id")
            .IsRequired();

        builder.Property(account => account.Status)
            .HasColumnName("status")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(account => account.Balance)
            .HasColumnName("balance")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(account => account.Version)
            .HasColumnName("version")
            .IsRequired();

        builder.Property(account => account.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(account => account.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(account => account.OwnerUserId)
            .HasDatabaseName("ix_accounts_read_model_owner_user_id");
    }
}

