using Banking.Application.Abstractions.Persistence;
using Banking.Application.Accounts;
using Banking.Application.Auth;
using Banking.Application.Exceptions;
using Banking.Domain.Accounts;
using Banking.Domain.ValueObjects;
using Banking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Accounts;

internal sealed class AccountService(
    BankingDbContext dbContext,
    IEventStore eventStore) : IAccountService
{
    private const string AggregateType = "Account";

    public async Task<AccountDetailsResponse> OpenAsync(
        OpenAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.RequesterRole != UserRoles.Customer)
        {
            throw new ForbiddenException("Only customers can open accounts.");
        }

        var account = Account.Open(AccountId.New(), new UserId(command.OwnerUserId));
        var metadata = CreateMetadata(command.OwnerUserId);

        await eventStore.AppendAsync(
            account.Id,
            AggregateType,
            expectedVersion: 0,
            account.UncommittedEvents,
            metadata,
            cancellationToken);

        var readModel = ToReadModel(account);
        dbContext.AccountsReadModel.Add(readModel);
        await dbContext.SaveChangesAsync(cancellationToken);

        account.ClearUncommittedEvents();

        return ToResponse(readModel);
    }

    public async Task<IReadOnlyCollection<AccountDetailsResponse>> GetAccountsAsync(
        AccountsQuery query,
        CancellationToken cancellationToken = default)
    {
        var accounts = query.RequesterRole == UserRoles.Admin
            ? await dbContext.AccountsReadModel
                .AsNoTracking()
                .OrderBy(account => account.CreatedAt)
                .ToListAsync(cancellationToken)
            : await dbContext.AccountsReadModel
                .AsNoTracking()
                .Where(account => account.OwnerUserId == query.RequesterUserId)
                .OrderBy(account => account.CreatedAt)
                .ToListAsync(cancellationToken);

        return accounts.Select(ToResponse).ToArray();
    }

    public async Task<AccountDetailsResponse> GetByIdAsync(
        GetAccountByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var readModel = await GetReadModelAsync(query.AccountId, cancellationToken);

        EnsureCanAccess(readModel, query.RequesterUserId, query.RequesterRole);

        return ToResponse(readModel);
    }

    public async Task<AccountDetailsResponse> ActivateAsync(
        ActivateAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin(command.RequesterRole);

        var account = await LoadAccountAsync(command.AccountId, cancellationToken);
        var expectedVersion = account.Version;

        account.Activate();

        await AppendAndProjectAsync(account, expectedVersion, command.RequesterUserId, cancellationToken);

        return ToResponse(await GetReadModelAsync(command.AccountId, cancellationToken));
    }

    public async Task<AccountDetailsResponse> DeactivateAsync(
        DeactivateAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        EnsureAdmin(command.RequesterRole);

        var account = await LoadAccountAsync(command.AccountId, cancellationToken);
        var expectedVersion = account.Version;

        account.Deactivate();

        await AppendAndProjectAsync(account, expectedVersion, command.RequesterUserId, cancellationToken);

        return ToResponse(await GetReadModelAsync(command.AccountId, cancellationToken));
    }

    private async Task<Account> LoadAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var events = await eventStore.LoadAsync(new AccountId(accountId), cancellationToken);

        if (events.Count == 0)
        {
            throw new NotFoundException("Account was not found.");
        }

        return Account.Rehydrate(events.Select(envelope => envelope.Event));
    }

    private async Task AppendAndProjectAsync(
        Account account,
        int expectedVersion,
        Guid requesterUserId,
        CancellationToken cancellationToken)
    {
        if (account.UncommittedEvents.Count == 0)
        {
            return;
        }

        await eventStore.AppendAsync(
            account.Id,
            AggregateType,
            expectedVersion,
            account.UncommittedEvents,
            CreateMetadata(requesterUserId),
            cancellationToken);

        var readModel = await GetReadModelAsync(account.Id.Value, cancellationToken);
        readModel.Status = account.Status.ToString();
        readModel.Balance = account.Balance.Amount;
        readModel.Version = account.Version;
        readModel.UpdatedAt = account.UpdatedAt;

        await dbContext.SaveChangesAsync(cancellationToken);

        account.ClearUncommittedEvents();
    }

    private async Task<AccountReadModel> GetReadModelAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return await dbContext.AccountsReadModel
            .SingleOrDefaultAsync(account => account.Id == accountId, cancellationToken)
            ?? throw new NotFoundException("Account was not found.");
    }

    private static EventMetadata CreateMetadata(Guid requesterUserId)
    {
        return new EventMetadata(
            Guid.NewGuid(),
            CausationId: null,
            new UserId(requesterUserId));
    }

    private static AccountReadModel ToReadModel(Account account)
    {
        return new AccountReadModel
        {
            Id = account.Id.Value,
            OwnerUserId = account.OwnerUserId.Value,
            Status = account.Status.ToString(),
            Balance = account.Balance.Amount,
            Version = account.Version,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt
        };
    }

    private static AccountDetailsResponse ToResponse(AccountReadModel account)
    {
        return new AccountDetailsResponse(
            account.Id,
            account.OwnerUserId,
            account.Status,
            account.Balance,
            account.Version,
            account.CreatedAt,
            account.UpdatedAt);
    }

    private static void EnsureAdmin(string requesterRole)
    {
        if (requesterRole != UserRoles.Admin)
        {
            throw new ForbiddenException("Only administrators can change account status.");
        }
    }

    private static void EnsureCanAccess(AccountReadModel account, Guid requesterUserId, string requesterRole)
    {
        if (requesterRole == UserRoles.Admin || account.OwnerUserId == requesterUserId)
        {
            return;
        }

        throw new ForbiddenException("Customers can access only their own accounts.");
    }
}

