namespace Banking.Api.Controllers;

public sealed record TransferRequest(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string? Description);

