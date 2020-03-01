using System.Threading.Tasks;
using Docker.DotNet;
using TestContainers.Containers;
using TestContainers.Integration.Tests.Networks.Fixtures;
using TestContainers.Networks;
using Xunit;

namespace TestContainers.Integration.Tests.Networks
{
    [Collection(UserDefinedNetworkTestCollection.CollectionName)]
    public class UserDefinedNetworkTests
    {
        private readonly UserDefinedNetworkFixture _fixture;

        private INetwork Network => _fixture.Network;

        private IDockerClient DockerClient => _fixture.DockerClient;

        public UserDefinedNetworkTests(UserDefinedNetworkFixture fixture)
        {
            _fixture = fixture;
        }

        public class ResolveTests : UserDefinedNetworkTests
        {
            public ResolveTests(UserDefinedNetworkFixture fixture) : base(fixture)
            {
            }

            [Fact]
            public async Task ShouldResolveImageCorrectly()
            {
                // act
                var actualNetworkId = await Network.ResolveAsync();

                // assert
                var actualNetwork = await DockerClient.Networks.InspectNetworkAsync(Network.NetworkId);
                Assert.Equal(actualNetwork.ID, actualNetworkId);
            }
        }

        public class WithContainer : UserDefinedNetworkTests
        {
            public WithContainer(UserDefinedNetworkFixture fixture) : base(fixture)
            {
            }

            [Fact]
            public async Task ShouldCreateAndStartContainerSuccessfully()
            {
                // arrange
                var container = new ContainerBuilder<GenericContainer>()
                    .ConfigureNetwork(Network)
                    .Build();

                // act
                await container.StartAsync();

                // assert
                Assert.Equal(Network.NetworkName, container.Network.NetworkName);
                await container.StopAsync();
            }

            [Fact]
            public async Task ShouldBeAbleToReachEachOther()
            {
                // arrange
                const string serverImageName = "jdkelley/busybox-echo-server:latest";
                const int serverPort = 1234;
                const string container1Text = "abcd";
                const string container1Alias = "container1";
                const string container2Text = "1234";
                const string container2Alias = "container2";

                var container1 = new ContainerBuilder<GenericContainer>()
                    .ConfigureDockerImageName(serverImageName)
                    .ConfigureNetwork(Network)
                    .ConfigureContainer((h, c) =>
                    {
                        c.NetWorkAliases.Add(container1Alias);
                    })
                    .Build();

                var container2 = new ContainerBuilder<GenericContainer>()
                    .ConfigureDockerImageName(serverImageName)
                    .ConfigureNetwork(Network)
                    .ConfigureContainer((h, c) =>
                    {
                        c.NetWorkAliases.Add(container2Alias);
                    })
                    .Build();

                // act
                await Task.WhenAll(
                    container1.StartAsync(),
                    container2.StartAsync());

                // assert
                var (out1, err1) =
                    await container1.ExecuteCommand("sh", "-c", $"echo -n {container1Text} | nc {container2Alias} {serverPort}");
                var (out2, err2) =
                    await container2.ExecuteCommand("sh", "-c", $"echo -n {container2Text} | nc {container1Alias} {serverPort}");

                Assert.Equal(container1Text, out1);
                Assert.Equal(container2Text, out2);
                await Task.WhenAll(
                    container1.StopAsync(),
                    container2.StopAsync());
            }
        }
    }
}
