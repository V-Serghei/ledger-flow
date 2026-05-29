namespace Banking.Application.Accounts;

public sealed record DeactivateAccountCommand(
    Guid AccountId,
    Guid RequesterUserId,
    string RequesterRole);

