namespace Banking.Application.Accounts;

public sealed record AccountsQuery(
    Guid RequesterUserId,
    string RequesterRole);

