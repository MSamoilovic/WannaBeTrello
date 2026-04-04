namespace Feezbow.Integration.Tests.Infrastructure;

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<IntegrationTestFixture>
{
    // Marker class. All test classes decorated with [Collection("Integration")]
    // will share a single IntegrationTestFixture instance (one container pair).
}
