using Xunit;

namespace TestContainers.Integration.Tests.Networks.Fixtures
{
    [CollectionDefinition(CollectionName)]
    public class UserDefinedNetworkTestCollection : ICollectionFixture<UserDefinedNetworkFixture>
    {
        public const string CollectionName = nameof(UserDefinedNetworkTestCollection);
    }
}
