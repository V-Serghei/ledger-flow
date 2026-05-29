using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Banking.Api.Controllers;
using Banking.Application.Accounts;
using Banking.Application.Deposits;
using Banking.Application.Pix;
using Banking.Application.Statements;
using Banking.Application.Transfers;

namespace Banking.IntegrationTests.Support;

public sealed class BankingApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<AuthResponse> RegisterAsync(string? email = null)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/auth/register", new
        {
            email = email ?? UniqueEmail(),
            password = "Password123!"
        });

        response.EnsureSuccessStatusCode();

        return await ReadAsync<AuthResponse>(response);
    }

    public async Task<AuthResponse> LoginAsync(string email)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email,
            password = "Password123!"
        });

        response.EnsureSuccessStatusCode();

        return await ReadAsync<AuthResponse>(response);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/auth/refresh", new
        {
            refreshToken
        });

        response.EnsureSuccessStatusCode();

        return await ReadAsync<AuthResponse>(response);
    }

    public async Task<AccountDetailsResponse> OpenAccountAsync(string token)
    {
        using var request = CreateRequest(HttpMethod.Post, "/api/v1/accounts", token);
        var response = await httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await ReadAsync<AccountDetailsResponse>(response);
    }

    public async Task<DepositResponse> DepositAsync(
        string token,
        Guid accountId,
        decimal amount,
        string idempotencyKey,
        string description = "integration deposit")
    {
        using var request = CreateRequest(HttpMethod.Post, "/api/v1/deposits", token, idempotencyKey, new
        {
            accountId,
            amount,
            description
        });

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await ReadAsync<DepositResponse>(response);
    }

    public async Task<TransferResponse> TransferAsync(
        string token,
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        string idempotencyKey,
        string description = "integration transfer")
    {
        using var request = CreateRequest(HttpMethod.Post, "/api/v1/transfers", token, idempotencyKey, new
        {
            sourceAccountId,
            destinationAccountId,
            amount,
            description
        });

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await ReadAsync<TransferResponse>(response);
    }

    public async Task<PixKeyResponse> RegisterPixKeyAsync(
        string token,
        Guid accountId,
        string key)
    {
        using var request = CreateRequest(HttpMethod.Post, "/api/v1/pix/keys", token, body: new
        {
            accountId,
            type = "Email",
            key
        });

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await ReadAsync<PixKeyResponse>(response);
    }

    public async Task<PixPaymentResponse> PayPixAsync(
        string token,
        Guid sourceAccountId,
        string destinationKey,
        decimal amount,
        string idempotencyKey)
    {
        using var request = CreateRequest(HttpMethod.Post, "/api/v1/pix/payments", token, idempotencyKey, new
        {
            sourceAccountId,
            destinationKey,
            amount,
            description = "integration pix payment"
        });

        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await ReadAsync<PixPaymentResponse>(response);
    }

    public async Task<PagedStatementResponse> GetStatementAsync(string token, Guid accountId)
    {
        using var request = CreateRequest(
            HttpMethod.Get,
            $"/api/v1/accounts/{accountId}/statement?page=1&pageSize=20",
            token);
        var response = await httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await ReadAsync<PagedStatementResponse>(response);
    }

    public static string UniqueEmail()
    {
        return $"customer-{Guid.NewGuid():N}@test.com";
    }

    public static async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions)
            ?? throw new InvalidOperationException($"Could not deserialize {typeof(T).Name}.");
    }

    public static HttpRequestMessage CreateRequest(
        HttpMethod method,
        string path,
        string? token = null,
        string? idempotencyKey = null,
        object? body = null)
    {
        var request = new HttpRequestMessage(method, path);

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            request.Headers.Add("Idempotency-Key", idempotencyKey);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        return request;
    }
}
