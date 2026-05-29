using Banking.Application.Abstractions.Clock;
using Banking.Application.Abstractions.Persistence;
using Banking.Infrastructure.Clock;
using Banking.Infrastructure.Persistence;
using Banking.Infrastructure.Persistence.EventStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Banking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' was not configured.");

        services.AddDbContext<BankingDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IEventStore, PostgresEventStore>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}

