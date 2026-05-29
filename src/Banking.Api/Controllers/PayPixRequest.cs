namespace Banking.Api.Controllers;

public sealed record PayPixRequest(
    Guid SourceAccountId,
    string DestinationKey,
    decimal Amount,
    string? Description);

