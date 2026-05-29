namespace Banking.Application.Statements;

public interface IStatementService
{
    Task<PagedStatementResponse> GetStatementAsync(
        GetStatementQuery query,
        CancellationToken cancellationToken = default);
}

