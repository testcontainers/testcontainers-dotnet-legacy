using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using TestContainers.Networks;
using TestContainers.Test.Utilities;
using Xunit;

namespace TestContainers.Integration.Tests.Networks.Fixtures
{
    public class UserDefinedNetworkFixture : IAsyncLifetime
    {
        public INetwork Network { get; }

        public IDockerClient DockerClient { get; }

        public UserDefinedNetworkFixture()
        {
            Network = new NetworkBuilder<UserDefinedNetwork>()
                .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection())
                .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection())
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureNetwork((context, network) =>
                {
                })
                .Build();

            DockerClient = ((UserDefinedNetwork) Network).DockerClient;
        }

        public async Task InitializeAsync()
        {
            await Network.ReapAsync();
        }

        public async Task DisposeAsync()
        {
            await Network.ReapAsync();
        }
    }
}
