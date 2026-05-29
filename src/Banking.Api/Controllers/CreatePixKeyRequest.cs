namespace Banking.Api.Controllers;

public sealed record CreatePixKeyRequest(
    Guid AccountId,
    string Type,
    string Key);

