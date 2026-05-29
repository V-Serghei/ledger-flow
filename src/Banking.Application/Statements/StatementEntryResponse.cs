namespace Banking.Application.Statements;

public sealed record StatementEntryResponse(
    Guid Id,
    Guid AccountId,
    Guid OperationId,
    string OperationType,
    string Direction,
    decimal Amount,
    decimal BalanceAfter,
    string Description,
    DateTimeOffset OccurredOn);

