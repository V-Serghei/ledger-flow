namespace Banking.Application.Accounts;

public sealed record OpenAccountCommand(
    Guid OwnerUserId,
    string RequesterRole);

