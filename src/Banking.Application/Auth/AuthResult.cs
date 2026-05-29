namespace Banking.Application.Auth;

public sealed record AuthResult(
    Guid UserId,
    string Email,
    string Role,
    string AccessToken,
    DateTimeOffset AccessTokenExpiresOn,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresOn);

