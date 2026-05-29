namespace Banking.Application.Deposits;

public sealed record DepositResponse(
    Guid AccountId,
    decimal Amount,
    decimal BalanceAfter,
    string Description,
    DateTimeOffset OccurredOn);

