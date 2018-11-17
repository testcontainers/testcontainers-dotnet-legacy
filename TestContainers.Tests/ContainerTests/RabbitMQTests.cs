using System.Threading.Tasks;
using Xunit;
using RabbitMQ.Client;
using TestContainers.Core.Builders;
using TestContainers.Core.Containers;

namespace TestContainers.Tests.ContainerTests
{
    public class RabbitMqFixture : IAsyncLifetime
    {
        public IConnection Connection => Container.Connection;
        private RabbitMqContainer Container { get; }

        public RabbitMqFixture() =>
            Container = new RabbitMqContainerBuilder()
                .WithUser("admin")
                .WithPassword("admin")
                .Build();

        public Task InitializeAsync() => Container.Start();

        public Task DisposeAsync() => Container.Stop();
    }

    public class RabbitMqTests : IClassFixture<RabbitMqFixture>
    {
        private readonly IConnection _connection;

        public RabbitMqTests(RabbitMqFixture fixture) => _connection = fixture.Connection;
        
        [Fact]
        public void OpenModelTest()
        {
            var model = _connection.CreateModel();
            
            Assert.True(model.IsOpen);
        }
    }
}