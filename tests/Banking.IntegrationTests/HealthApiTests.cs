using Banking.IntegrationTests.Support;
using FluentAssertions;

namespace Banking.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class HealthApiTests(BankingApiFactory factory)
{
    [Fact]
    public async Task should_return_live_health()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
