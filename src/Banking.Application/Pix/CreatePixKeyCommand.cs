namespace Banking.Application.Pix;

public sealed record CreatePixKeyCommand(
    Guid AccountId,
    string Type,
    string Key,
    Guid RequesterUserId,
    string RequesterRole);

