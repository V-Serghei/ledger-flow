using Banking.Application.Abstractions.Clock;
using Banking.Application.Abstractions.Persistence;
using Banking.Application.Accounts;
using Banking.Application.Auth;
using Banking.Application.Deposits;
using Banking.Application.Transfers;
using Banking.Infrastructure.Accounts;
using Banking.Infrastructure.Auth;
using Banking.Infrastructure.Deposits;
using Banking.Infrastructure.Transfers;
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
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDepositService, DepositService>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IRefreshTokenProtector, Sha256RefreshTokenProtector>();
        services.AddSingleton<IClock, SystemClock>();

        services.Configure<JwtOptions>(options =>
        {
            var section = configuration.GetSection(JwtOptions.SectionName);
            options.Issuer = section["Issuer"] ?? string.Empty;
            options.Audience = section["Audience"] ?? string.Empty;
            options.SigningKey = section["SigningKey"] ?? string.Empty;
            options.AccessTokenMinutes = int.TryParse(section["AccessTokenMinutes"], out var accessTokenMinutes)
                ? accessTokenMinutes
                : options.AccessTokenMinutes;
            options.RefreshTokenDays = int.TryParse(section["RefreshTokenDays"], out var refreshTokenDays)
                ? refreshTokenDays
                : options.RefreshTokenDays;
        });

        return services;
    }
}
