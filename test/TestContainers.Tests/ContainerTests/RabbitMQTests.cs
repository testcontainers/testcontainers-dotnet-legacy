using System.Threading.Tasks;
using RabbitMQ.Client;
using TestContainers.Core.Builders;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.ContainerTests
{
    public class RabbitMQFixture : IAsyncLifetime
    {
        public IConnection Connection => Container.Connection;
        RabbitMQContainer Container { get; }

        public RabbitMQFixture() =>
            Container = new GenericContainerBuilder<RabbitMQContainer>()
                .Begin()
                .WithImage($"{RabbitMQContainer.IMAGE}:{RabbitMQContainer.TAG}")
                .WithExposedPorts(RabbitMQContainer.PORT)
                .Build();

        public Task InitializeAsync() => Container.Start();

        public Task DisposeAsync() => Container.Stop();
    }

    public class RabbitMQTests : IClassFixture<RabbitMQFixture>
    {
        RabbitMQFixture _fixture;

        public RabbitMQTests(RabbitMQFixture fixture) => _fixture = fixture;

        [Fact]
        public void OpenModelTest()
        {
            var model = _fixture.Connection.CreateModel();

            Assert.True(model.IsOpen);
        }
    }
}
