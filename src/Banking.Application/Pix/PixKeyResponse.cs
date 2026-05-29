namespace Banking.Application.Pix;

public sealed record PixKeyResponse(
    Guid Id,
    Guid AccountId,
    Guid OwnerUserId,
    string Type,
    string Key,
    DateTimeOffset CreatedAt);

