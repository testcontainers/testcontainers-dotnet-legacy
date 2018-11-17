using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using TestContainers.Core.Builders;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.ContainerTests
{
    public class GenericContainerFixture : IAsyncLifetime
    {
        public ContainerInspectResponse ContainerInfo => _container.ContainerInfo;
        private readonly GenericContainer _container;

        public GenericContainerFixture() => _container = new GenericContainerBuilder()
            .WithImage("alpine:latest")
            .WithLabel("your.custom", "label")
            .Build();

        public Task InitializeAsync() => _container.Start();

        public Task DisposeAsync() => _container.Stop();
    }

    public class GenericContainerTests : IClassFixture<GenericContainerFixture>
    {
        readonly ContainerInspectResponse _containerInfo;

        public GenericContainerTests(GenericContainerFixture fixture) => _containerInfo = fixture.ContainerInfo;
        
        [Fact]
        public void CustomLabelTest()
        {
            var label = _containerInfo.Config.Labels.Single();
            Assert.Equal("your.custom", label.Key);
            Assert.Equal("label", label.Value);
        }
    }

}