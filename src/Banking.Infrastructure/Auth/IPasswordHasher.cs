namespace Banking.Infrastructure.Auth;

internal interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}

