namespace Banking.Application.Accounts;

public sealed record ActivateAccountCommand(
    Guid AccountId,
    Guid RequesterUserId,
    string RequesterRole);

