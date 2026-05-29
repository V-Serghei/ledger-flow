using System.Text.Json;
using Banking.Application.Abstractions.Persistence;
using Banking.Application.Auth;
using Banking.Application.Exceptions;
using Banking.Application.Messaging;
using Banking.Application.Messaging.IntegrationEvents;
using Banking.Application.Transfers;
using Banking.Domain.Accounts;
using Banking.Domain.Exceptions;
using Banking.Domain.ValueObjects;
using Banking.Infrastructure.Accounts;
using Banking.Infrastructure.Idempotency;
using Banking.Infrastructure.Persistence;
using Banking.Infrastructure.Statements;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Transfers;

internal sealed class TransferService(
    BankingDbContext dbContext,
    IEventStore eventStore,
    IOutboxWriter outboxWriter) : ITransferService
{
    private const string AggregateType = "Account";
    private const string OperationType = "Transfer";
    private const string CompletedStatus = "Completed";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<TransferResponse> TransferAsync(
        TransferMoneyCommand command,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(command.IdempotencyKey);
        var requestHash = RequestHasher.Hash(command);

        var existingRecord = await dbContext.IdempotencyRecords
            .AsNoTracking()
            .SingleOrDefaultAsync(record =>
                record.UserId == command.RequesterUserId
                && record.OperationType == OperationType
                && record.IdempotencyKey == command.IdempotencyKey,
                cancellationToken);

        if (existingRecord is not null)
        {
            if (existingRecord.RequestHash != requestHash)
            {
                throw new IdempotencyConflictException(
                    "The same Idempotency-Key was already used with a different request payload.");
            }

            return JsonSerializer.Deserialize<TransferResponse>(existingRecord.ResponsePayload, JsonOptions)
                ?? throw new InvalidOperationException("Stored idempotency response could not be deserialized.");
        }

        if (command.SourceAccountId == command.DestinationAccountId)
        {
            throw new SameAccountTransferException("Source and destination accounts must be different.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var sourceReadModel = await GetAccountReadModelForUpdateAsync(command.SourceAccountId, cancellationToken);
        var destinationReadModel = await GetAccountReadModelAsync(command.DestinationAccountId, cancellationToken);

        EnsureCanTransfer(sourceReadModel, command.RequesterUserId, command.RequesterRole);

        var sourceEvents = await eventStore.LoadAsync(new AccountId(command.SourceAccountId), cancellationToken);
        var destinationEvents = await eventStore.LoadAsync(new AccountId(command.DestinationAccountId), cancellationToken);
        var source = Account.Rehydrate(sourceEvents.Select(envelope => envelope.Event));
        var destination = Account.Rehydrate(destinationEvents.Select(envelope => envelope.Event));
        var sourceExpectedVersion = source.Version;
        var destinationExpectedVersion = destination.Version;
        var amount = Money.FromOperationAmount(command.Amount);

        source.TransferTo(destination, amount, command.Description);

        await eventStore.AppendAsync(
            source.Id,
            AggregateType,
            sourceExpectedVersion,
            source.UncommittedEvents,
            CreateMetadata(command.RequesterUserId),
            cancellationToken);

        await eventStore.AppendAsync(
            destination.Id,
            AggregateType,
            destinationExpectedVersion,
            destination.UncommittedEvents,
            CreateMetadata(command.RequesterUserId),
            cancellationToken);

        UpdateAccountReadModel(sourceReadModel, source);
        UpdateAccountReadModel(destinationReadModel, destination);

        var response = new TransferResponse(
            Guid.NewGuid(),
            source.Id.Value,
            destination.Id.Value,
            amount.Amount,
            CompletedStatus,
            command.Description ?? string.Empty,
            DateTimeOffset.UtcNow);

        var correlationId = Guid.NewGuid();
        outboxWriter.Add(
            new TransferCompletedIntegrationEvent(
                response.TransferId,
                response.SourceAccountId,
                response.DestinationAccountId,
                response.Amount,
                response.Description,
                response.OccurredOn),
            correlationId);

        dbContext.StatementEntriesReadModel.AddRange(
            new StatementEntryReadModel
            {
                Id = Guid.NewGuid(),
                AccountId = source.Id.Value,
                OperationId = response.TransferId,
                OperationType = OperationType,
                Direction = "Debit",
                Amount = response.Amount,
                BalanceAfter = source.Balance.Amount,
                Description = response.Description,
                OccurredOn = response.OccurredOn
            },
            new StatementEntryReadModel
            {
                Id = Guid.NewGuid(),
                AccountId = destination.Id.Value,
                OperationId = response.TransferId,
                OperationType = OperationType,
                Direction = "Credit",
                Amount = response.Amount,
                BalanceAfter = destination.Balance.Amount,
                Description = response.Description,
                OccurredOn = response.OccurredOn
            });

        dbContext.TransfersReadModel.Add(new TransferReadModel
        {
            Id = response.TransferId,
            SourceAccountId = response.SourceAccountId,
            DestinationAccountId = response.DestinationAccountId,
            OwnerUserId = command.RequesterUserId,
            Amount = response.Amount,
            Status = response.Status,
            Description = response.Description,
            OccurredOn = response.OccurredOn
        });

        dbContext.IdempotencyRecords.Add(new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            UserId = command.RequesterUserId,
            OperationType = OperationType,
            IdempotencyKey = command.IdempotencyKey,
            RequestHash = requestHash,
            ResponsePayload = JsonSerializer.Serialize(response, JsonOptions),
            StatusCode = StatusCodes.Created,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        source.ClearUncommittedEvents();
        destination.ClearUncommittedEvents();

        return response;
    }

    public async Task<TransferResponse> GetByIdAsync(
        GetTransferByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var transfer = await dbContext.TransfersReadModel
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == query.TransferId, cancellationToken)
            ?? throw new NotFoundException("Transfer was not found.");

        if (query.RequesterRole != UserRoles.Admin && transfer.OwnerUserId != query.RequesterUserId)
        {
            throw new ForbiddenException("Customers can access only their own transfers.");
        }

        return ToResponse(transfer);
    }

    private async Task<AccountReadModel> GetAccountReadModelAsync(Guid accountId, CancellationToken cancellationToken)
    {
        return await dbContext.AccountsReadModel
            .SingleOrDefaultAsync(account => account.Id == accountId, cancellationToken)
            ?? throw new NotFoundException("Account was not found.");
    }

    private async Task<AccountReadModel> GetAccountReadModelForUpdateAsync(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        return await dbContext.AccountsReadModel
            .FromSqlInterpolated($"SELECT * FROM accounts_read_model WHERE id = {accountId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Account was not found.");
    }

    private static void UpdateAccountReadModel(AccountReadModel readModel, Account account)
    {
        readModel.Status = account.Status.ToString();
        readModel.Balance = account.Balance.Amount;
        readModel.Version = account.Version;
        readModel.UpdatedAt = account.UpdatedAt;
    }

    private static EventMetadata CreateMetadata(Guid requesterUserId)
    {
        return new EventMetadata(
            Guid.NewGuid(),
            CausationId: null,
            new UserId(requesterUserId));
    }

    private static void EnsureIdempotencyKey(string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new ArgumentException("Idempotency-Key header is required.", nameof(idempotencyKey));
        }
    }

    private static void EnsureCanTransfer(AccountReadModel sourceAccount, Guid requesterUserId, string requesterRole)
    {
        if (requesterRole == UserRoles.Customer && sourceAccount.OwnerUserId == requesterUserId)
        {
            return;
        }

        throw new ForbiddenException("Customers can transfer only from their own accounts.");
    }

    private static TransferResponse ToResponse(TransferReadModel transfer)
    {
        return new TransferResponse(
            transfer.Id,
            transfer.SourceAccountId,
            transfer.DestinationAccountId,
            transfer.Amount,
            transfer.Status,
            transfer.Description,
            transfer.OccurredOn);
    }

    private static class StatusCodes
    {
        public const int Created = 201;
    }
}
