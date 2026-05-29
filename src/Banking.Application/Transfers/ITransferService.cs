namespace Banking.Application.Transfers;

public interface ITransferService
{
    Task<TransferResponse> TransferAsync(
        TransferMoneyCommand command,
        CancellationToken cancellationToken = default);

    Task<TransferResponse> GetByIdAsync(
        GetTransferByIdQuery query,
        CancellationToken cancellationToken = default);
}

