namespace Banking.Application.Accounts;

public interface IAccountService
{
    Task<AccountDetailsResponse> OpenAsync(
        OpenAccountCommand command,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<AccountDetailsResponse>> GetAccountsAsync(
        AccountsQuery query,
        CancellationToken cancellationToken = default);

    Task<AccountDetailsResponse> GetByIdAsync(
        GetAccountByIdQuery query,
        CancellationToken cancellationToken = default);

    Task<AccountDetailsResponse> ActivateAsync(
        ActivateAccountCommand command,
        CancellationToken cancellationToken = default);

    Task<AccountDetailsResponse> DeactivateAsync(
        DeactivateAccountCommand command,
        CancellationToken cancellationToken = default);
}

