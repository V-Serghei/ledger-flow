namespace Banking.IntegrationTests.Support;

[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<BankingApiFactory>
{
    public const string Name = "Integration";
}
