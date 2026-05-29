using Banking.Infrastructure.Auth;
using Banking.Infrastructure.Accounts;
using Banking.Infrastructure.Idempotency;
using Banking.Infrastructure.Persistence.EventStore;
using Banking.Infrastructure.Transfers;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Persistence;

public sealed class BankingDbContext(DbContextOptions<BankingDbContext> options) : DbContext(options)
{
    public DbSet<EventStoreRecord> EventStore => Set<EventStoreRecord>();

    public DbSet<UserRecord> Users => Set<UserRecord>();

    public DbSet<RefreshTokenRecord> RefreshTokens => Set<RefreshTokenRecord>();

    public DbSet<AccountReadModel> AccountsReadModel => Set<AccountReadModel>();

    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    public DbSet<TransferReadModel> TransfersReadModel => Set<TransferReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BankingDbContext).Assembly);
    }
}
