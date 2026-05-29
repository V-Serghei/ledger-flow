using Banking.Application.Auth;
using Banking.Application.Exceptions;
using Banking.Application.Messaging;
using Banking.Application.Messaging.IntegrationEvents;
using Banking.Application.Pix;
using Banking.Application.Transfers;
using Banking.Infrastructure.Accounts;
using Banking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Pix;

internal sealed class PixService(
    BankingDbContext dbContext,
    ITransferService transferService,
    IOutboxWriter outboxWriter) : IPixService
{
    private static readonly HashSet<string> SupportedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Email",
        "FakeCpf",
        "Phone",
        "Random"
    };

    public async Task<PixKeyResponse> CreateKeyAsync(
        CreatePixKeyCommand command,
        CancellationToken cancellationToken = default)
    {
        EnsureSupportedType(command.Type);

        var account = await dbContext.AccountsReadModel
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == command.AccountId, cancellationToken)
            ?? throw new NotFoundException("Account was not found.");

        EnsureCanCreateKey(account, command.RequesterUserId, command.RequesterRole);

        var normalizedKey = NormalizeKey(command.Key);
        var keyAlreadyExists = await dbContext.PixKeys
            .AnyAsync(candidate => candidate.NormalizedKey == normalizedKey, cancellationToken);

        if (keyAlreadyExists)
        {
            throw new ConflictException("PIX key is already registered.");
        }

        var pixKey = new PixKeyRecord
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            OwnerUserId = account.OwnerUserId,
            Type = NormalizeType(command.Type),
            Key = command.Key.Trim(),
            NormalizedKey = normalizedKey,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.PixKeys.Add(pixKey);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(pixKey);
    }

    public async Task<IReadOnlyCollection<PixKeyResponse>> GetKeysAsync(
        GetPixKeysQuery query,
        CancellationToken cancellationToken = default)
    {
        var keys = query.RequesterRole == UserRoles.Admin
            ? await dbContext.PixKeys.AsNoTracking().OrderBy(key => key.CreatedAt).ToListAsync(cancellationToken)
            : await dbContext.PixKeys
                .AsNoTracking()
                .Where(key => key.OwnerUserId == query.RequesterUserId)
                .OrderBy(key => key.CreatedAt)
                .ToListAsync(cancellationToken);

        return keys.Select(ToResponse).ToArray();
    }

    public async Task<PixPaymentResponse> PayAsync(
        PayPixCommand command,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.IdempotencyKey))
        {
            throw new ArgumentException("Idempotency-Key header is required.", nameof(command.IdempotencyKey));
        }

        var destinationKey = NormalizeKey(command.DestinationKey);
        var pixKey = await dbContext.PixKeys
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.NormalizedKey == destinationKey, cancellationToken)
            ?? throw new NotFoundException("PIX key was not found.");

        var transfer = await transferService.TransferAsync(
            new TransferMoneyCommand(
                command.SourceAccountId,
                pixKey.AccountId,
                command.Amount,
                command.Description,
                $"pix:{command.IdempotencyKey}",
                command.RequesterUserId,
                command.RequesterRole),
            cancellationToken);

        var response = new PixPaymentResponse(
            Guid.NewGuid(),
            transfer.TransferId,
            transfer.SourceAccountId,
            transfer.DestinationAccountId,
            pixKey.Key,
            transfer.Amount,
            transfer.Status,
            transfer.Description,
            transfer.OccurredOn);

        outboxWriter.Add(
            new PixPaymentCompletedIntegrationEvent(
                response.PaymentId,
                response.TransferId,
                response.SourceAccountId,
                response.DestinationAccountId,
                response.DestinationKey,
                response.Amount,
                response.OccurredOn),
            Guid.NewGuid());

        await dbContext.SaveChangesAsync(cancellationToken);

        return response;
    }

    private static PixKeyResponse ToResponse(PixKeyRecord pixKey)
    {
        return new PixKeyResponse(
            pixKey.Id,
            pixKey.AccountId,
            pixKey.OwnerUserId,
            pixKey.Type,
            pixKey.Key,
            pixKey.CreatedAt);
    }

    private static void EnsureCanCreateKey(AccountReadModel account, Guid requesterUserId, string requesterRole)
    {
        if (requesterRole == UserRoles.Admin || account.OwnerUserId == requesterUserId)
        {
            return;
        }

        throw new ForbiddenException("Customers can create PIX keys only for their own accounts.");
    }

    private static void EnsureSupportedType(string type)
    {
        if (!SupportedTypes.Contains(type))
        {
            throw new ArgumentException("PIX key type must be Email, FakeCpf, Phone, or Random.", nameof(type));
        }
    }

    private static string NormalizeType(string type)
    {
        return SupportedTypes.Single(candidate => string.Equals(candidate, type, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("PIX key is required.", nameof(key));
        }

        return key.Trim().ToLowerInvariant();
    }
}
