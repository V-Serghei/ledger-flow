using Banking.Application.Auth;
using Banking.Infrastructure.Auth;
using Banking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Banking.Infrastructure.Seed;

public static class DevelopmentDataSeeder
{
    public static async Task SeedDevelopmentDataAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BankingDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await SeedUserAsync(
            dbContext,
            passwordHasher,
            "customer@test.com",
            UserRoles.Customer);

        await SeedUserAsync(
            dbContext,
            passwordHasher,
            "admin@test.com",
            UserRoles.Admin);

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedUserAsync(
        BankingDbContext dbContext,
        IPasswordHasher passwordHasher,
        string email,
        string role)
    {
        var normalizedEmail = email.ToUpperInvariant();

        if (await dbContext.Users.AnyAsync(user => user.NormalizedEmail == normalizedEmail))
        {
            return;
        }

        dbContext.Users.Add(new UserRecord
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = normalizedEmail,
            PasswordHash = passwordHasher.Hash("Password123!"),
            Role = role,
            CreatedAt = DateTimeOffset.UtcNow
        });
    }
}
