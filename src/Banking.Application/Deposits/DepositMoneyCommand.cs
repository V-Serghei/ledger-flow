namespace Banking.Application.Deposits;

public sealed record DepositMoneyCommand(
    Guid AccountId,
    decimal Amount,
    string? Description,
    string IdempotencyKey,
    Guid RequesterUserId,
    string RequesterRole);

