using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Banking.Infrastructure.Idempotency;

internal static class RequestHasher
{
    public static string Hash<TRequest>(TRequest request)
    {
        var json = JsonSerializer.Serialize(request, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(json));

        return Base64UrlEncoder.Encode(bytes);
    }
}

