namespace Banking.Infrastructure.Statements;

public sealed class StatementEntryReadModel
{
    public Guid Id { get; init; }

    public Guid AccountId { get; init; }

    public Guid OperationId { get; init; }

    public string OperationType { get; init; } = string.Empty;

    public string Direction { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public decimal BalanceAfter { get; init; }

    public string Description { get; init; } = string.Empty;

    public DateTimeOffset OccurredOn { get; init; }
}

