namespace Banking.Application.Pix;

public sealed record PayPixCommand(
    Guid SourceAccountId,
    string DestinationKey,
    decimal Amount,
    string? Description,
    string IdempotencyKey,
    Guid RequesterUserId,
    string RequesterRole);

