namespace Banking.Application.Accounts;

public sealed record AccountDetailsResponse(
    Guid AccountId,
    Guid OwnerUserId,
    string Status,
    decimal Balance,
    int Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

