using Xunit;

namespace TestContainers.Integration.Tests.Transferables.Fixtures
{
    [CollectionDefinition(CollectionName)]
    public class TransferablePathTestCollection : ICollectionFixture<TransferablePathFixture>
    {
        public const string CollectionName = nameof(TransferablePathTestCollection);
    }
}
