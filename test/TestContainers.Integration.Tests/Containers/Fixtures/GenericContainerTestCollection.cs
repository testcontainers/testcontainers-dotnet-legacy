using Xunit;

namespace TestContainers.Integration.Tests.Containers.Fixtures
{
    [CollectionDefinition(CollectionName)]
    public class GenericContainerTestCollection : ICollectionFixture<GenericContainerFixture>
    {
        public const string CollectionName = nameof(GenericContainerTestCollection);
    }
}
