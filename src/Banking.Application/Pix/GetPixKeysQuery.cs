namespace Banking.Application.Pix;

public sealed record GetPixKeysQuery(
    Guid RequesterUserId,
    string RequesterRole);

