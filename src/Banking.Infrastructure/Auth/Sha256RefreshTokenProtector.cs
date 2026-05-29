using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Banking.Infrastructure.Auth;

internal sealed class Sha256RefreshTokenProtector : IRefreshTokenProtector
{
    public string Generate()
    {
        return Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
    }

    public string Hash(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));

        return Base64UrlEncoder.Encode(bytes);
    }
}

