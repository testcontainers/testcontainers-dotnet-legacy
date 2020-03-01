using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using TestContainers.Containers;
using TestContainers.Images;
using TestContainers.Test.Utilities;
using Xunit;

namespace TestContainers.Integration.Tests.Images.Fixtures
{
    public class GenericImageFixture : IAsyncLifetime
    {
        public IImage Image { get; }

        public IDockerClient DockerClient { get; }

        public GenericImageFixture()
        {
            Image = new ImageBuilder<GenericImage>()
                .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection())
                .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection())
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureImage((context, image) =>
                {
                    image.ImageName = $"{GenericContainer.DefaultImageName}:{GenericContainer.DefaultTagName}";
                })
                .Build();

            DockerClient = ((GenericImage) Image).DockerClient;
        }

        public async Task InitializeAsync()
        {
            await Image.ReapAsync();
        }

        public async Task DisposeAsync()
        {
            await Image.ReapAsync();
        }
    }
}
