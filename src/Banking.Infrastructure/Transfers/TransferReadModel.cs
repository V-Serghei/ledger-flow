namespace Banking.Infrastructure.Transfers;

public sealed class TransferReadModel
{
    public Guid Id { get; init; }

    public Guid SourceAccountId { get; init; }

    public Guid DestinationAccountId { get; init; }

    public Guid OwnerUserId { get; init; }

    public decimal Amount { get; init; }

    public string Status { get; set; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public DateTimeOffset OccurredOn { get; init; }
}

