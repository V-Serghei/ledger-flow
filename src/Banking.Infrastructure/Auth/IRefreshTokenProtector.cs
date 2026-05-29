namespace Banking.Infrastructure.Auth;

internal interface IRefreshTokenProtector
{
    string Generate();

    string Hash(string refreshToken);
}

