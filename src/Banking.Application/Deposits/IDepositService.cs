namespace Banking.Application.Deposits;

public interface IDepositService
{
    Task<DepositResponse> DepositAsync(
        DepositMoneyCommand command,
        CancellationToken cancellationToken = default);
}

