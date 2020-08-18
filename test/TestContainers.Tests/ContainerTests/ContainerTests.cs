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
        public ContainerInspectResponse ContainerInfo => _container.ContainerInspectResponse;
        readonly Container _container;

        public GenericContainerFixture() => _container = new GenericContainerBuilder<Container>()
            .Begin()
            .WithImage("alpine:latest")
            .WithLabel(("your.custom", "label"))
            .Build();

        public Task InitializeAsync() => _container.Start();

        public Task DisposeAsync() => _container.Stop();

    }

    public class GenericContainerTests : IClassFixture<GenericContainerFixture>
    {
        readonly ContainerInspectResponse _containerInfo;
        public GenericContainerTests(GenericContainerFixture fixture) => _containerInfo = fixture.ContainerInfo;

        [Fact]
        public void CustomLabelTest() => Assert.Equal("label", _containerInfo.Config.Labels["your.custom"]);

        [Theory]
        [InlineData("alpine:latest", "alpine:latest")]
        [InlineData("alpine", "alpine:latest")]
        [InlineData("alpine:hello", "alpine:hello")]
        [InlineData("test.de:55/alpine:latest", "test.de:55/alpine:latest")]
        [InlineData("test.de:55/alpine", "test.de:55/alpine:latest")]
        public void WithImage_ExtractFromImageAndTag(string path, string tag)
        {
            var container = new GenericContainerBuilder<Container>()
                .Begin()
                .WithImage(path)
                .Build();

            Assert.Equal(tag, container.DockerImageName);
        }

        [Fact]
        public async Task WithExecCommand()
        {
            var container = new GenericContainerBuilder<Container>()
                .Begin()
                .WithImage("alpine:latest")
                .Build();

            await container.Start();

            var execCommand = new[]
            {
                "/bin/sh",
                "-c",
                "ls"
            };

            await container.ExecuteCommand(execCommand);
            await container.Stop();
        }

        [Fact]
        public void WithEnv_MultipleCallsWithDifferentValuesAppendToEnvVariables()
        {
            var expectedEnvVariables = new[]
            {
                ("var1", "val1"),
                ("var2", "val2")
            };

            var container = new GenericContainerBuilder<Container>()
                .Begin()
                .WithImage("anyimage")
                .WithEnv(expectedEnvVariables[0])
                .WithEnv(expectedEnvVariables[1])
                .Build();

            Assert.Equal(expectedEnvVariables, container.EnvironmentVariables);
        }
    }
}
