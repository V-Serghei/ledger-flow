namespace Banking.Application.Pix;

public interface IPixService
{
    Task<PixKeyResponse> CreateKeyAsync(
        CreatePixKeyCommand command,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PixKeyResponse>> GetKeysAsync(
        GetPixKeysQuery query,
        CancellationToken cancellationToken = default);

    Task<PixPaymentResponse> PayAsync(
        PayPixCommand command,
        CancellationToken cancellationToken = default);
}

