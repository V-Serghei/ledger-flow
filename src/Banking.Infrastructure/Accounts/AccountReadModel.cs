namespace Banking.Infrastructure.Accounts;

public sealed class AccountReadModel
{
    public Guid Id { get; init; }

    public Guid OwnerUserId { get; init; }

    public string Status { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public int Version { get; set; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; set; }
}

