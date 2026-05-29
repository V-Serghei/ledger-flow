namespace Banking.Api.Controllers;

public sealed record DepositRequest(
    Guid AccountId,
    decimal Amount,
    string? Description);

