using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Statements;

internal sealed class StatementEntryReadModelConfiguration : IEntityTypeConfiguration<StatementEntryReadModel>
{
    public void Configure(EntityTypeBuilder<StatementEntryReadModel> builder)
    {
        builder.ToTable("statement_entries_read_model");

        builder.HasKey(entry => entry.Id);

        builder.Property(entry => entry.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(entry => entry.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(entry => entry.OperationId)
            .HasColumnName("operation_id")
            .IsRequired();

        builder.Property(entry => entry.OperationType)
            .HasColumnName("operation_type")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(entry => entry.Direction)
            .HasColumnName("direction")
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(entry => entry.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(entry => entry.BalanceAfter)
            .HasColumnName("balance_after")
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(entry => entry.Description)
            .HasColumnName("description")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(entry => entry.OccurredOn)
            .HasColumnName("occurred_on")
            .IsRequired();

        builder.HasIndex(entry => new { entry.AccountId, entry.OccurredOn })
            .HasDatabaseName("ix_statement_entries_account_id_occurred_on");

        builder.HasIndex(entry => entry.OperationId)
            .HasDatabaseName("ix_statement_entries_operation_id");
    }
}

