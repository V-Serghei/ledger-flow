namespace Banking.Application.Accounts;

public sealed record GetAccountByIdQuery(
    Guid AccountId,
    Guid RequesterUserId,
    string RequesterRole);

