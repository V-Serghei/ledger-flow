using FluentAssertions;
using Banking.IntegrationTests.Support;

namespace Banking.IntegrationTests;

public sealed class HealthApiTests(BankingApiFactory factory) : IClassFixture<BankingApiFactory>
{
    [Fact]
    public async Task should_return_live_health()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
