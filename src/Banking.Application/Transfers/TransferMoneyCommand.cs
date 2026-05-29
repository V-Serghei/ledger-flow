namespace Banking.Application.Transfers;

public sealed record TransferMoneyCommand(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string? Description,
    string IdempotencyKey,
    Guid RequesterUserId,
    string RequesterRole);

