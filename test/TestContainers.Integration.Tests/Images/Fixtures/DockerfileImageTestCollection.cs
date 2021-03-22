using Xunit;

namespace TestContainers.Integration.Tests.Images.Fixtures
{
    [CollectionDefinition(CollectionName)]
    public class DockerfileImageTestCollection : ICollectionFixture<DockerfileImageFixture>
    {
        public const string CollectionName = nameof(DockerfileImageTestCollection);
    }
}
