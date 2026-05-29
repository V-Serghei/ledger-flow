namespace Banking.Application.Transfers;

public sealed record TransferResponse(
    Guid TransferId,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string Status,
    string Description,
    DateTimeOffset OccurredOn);

