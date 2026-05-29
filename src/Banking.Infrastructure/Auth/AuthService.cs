using Banking.Application.Abstractions.Clock;
using Banking.Application.Auth;
using Banking.Application.Exceptions;
using Banking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Banking.Infrastructure.Auth;

internal sealed class AuthService(
    BankingDbContext dbContext,
    IPasswordHasher passwordHasher,
    IRefreshTokenProtector refreshTokenProtector,
    IClock clock,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    public async Task<AuthResult> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        EnsurePasswordIsValid(request.Password);

        var emailAlreadyExists = await dbContext.Users
            .AnyAsync(user => user.NormalizedEmail == email, cancellationToken);

        if (emailAlreadyExists)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var user = new UserRecord
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = UserRoles.Customer,
            CreatedAt = clock.UtcNow
        };

        dbContext.Users.Add(user);

        var result = IssueTokens(user);

        dbContext.RefreshTokens.Add(CreateRefreshTokenRecord(user.Id, result.RefreshToken, result.RefreshTokenExpiresOn));

        await dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<AuthResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var user = await dbContext.Users
            .SingleOrDefaultAsync(candidate => candidate.NormalizedEmail == email, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AuthenticationFailedException("Invalid email or password.");
        }

        var result = IssueTokens(user);

        dbContext.RefreshTokens.Add(CreateRefreshTokenRecord(user.Id, result.RefreshToken, result.RefreshTokenExpiresOn));

        await dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task<AuthResult> RefreshAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = refreshTokenProtector.Hash(request.RefreshToken);
        var refreshToken = await dbContext.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (refreshToken is null || !refreshToken.IsActive(clock.UtcNow))
        {
            throw new AuthenticationFailedException("Refresh token is invalid or expired.");
        }

        refreshToken.RevokedOn = clock.UtcNow;

        var result = IssueTokens(refreshToken.User);
        refreshToken.ReplacedByTokenHash = refreshTokenProtector.Hash(result.RefreshToken);

        dbContext.RefreshTokens.Add(CreateRefreshTokenRecord(
            refreshToken.UserId,
            result.RefreshToken,
            result.RefreshTokenExpiresOn));

        await dbContext.SaveChangesAsync(cancellationToken);

        return result;
    }

    public async Task LogoutAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = refreshTokenProtector.Hash(request.RefreshToken);
        var refreshToken = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (refreshToken is not null && refreshToken.RevokedOn is null)
        {
            refreshToken.RevokedOn = clock.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private AuthResult IssueTokens(UserRecord user)
    {
        var options = jwtOptions.Value;
        var accessTokenExpiresOn = clock.UtcNow.AddMinutes(options.AccessTokenMinutes);
        var refreshTokenExpiresOn = clock.UtcNow.AddDays(options.RefreshTokenDays);

        return new AuthResult(
            user.Id,
            user.Email,
            user.Role,
            JwtTokenFactory.Create(user, options, accessTokenExpiresOn),
            accessTokenExpiresOn,
            refreshTokenProtector.Generate(),
            refreshTokenExpiresOn);
    }

    private RefreshTokenRecord CreateRefreshTokenRecord(
        Guid userId,
        string refreshToken,
        DateTimeOffset expiresOn)
    {
        return new RefreshTokenRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = refreshTokenProtector.Hash(refreshToken),
            CreatedAt = clock.UtcNow,
            ExpiresOn = expiresOn
        };
    }

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        return email.Trim().ToLowerInvariant();
    }

    private static void EnsurePasswordIsValid(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            throw new ArgumentException("Password must contain at least 8 characters.", nameof(password));
        }
    }
}

