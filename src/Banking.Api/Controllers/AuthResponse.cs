using Banking.Application.Auth;

namespace Banking.Api.Controllers;

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    string Role,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresOn,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresOn)
{
    public static AuthResponse FromResult(AuthResult result)
    {
        return new AuthResponse(
            result.UserId,
            result.Email,
            result.Role,
            result.AccessToken,
            result.AccessTokenExpiresOn,
            result.RefreshToken,
            result.RefreshTokenExpiresOn);
    }
}

