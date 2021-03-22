using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestContainers.Containers;
using TestContainers.Images;
using TestContainers.Images.Builders;
using TestContainers.Internal.Builders;
using TestContainers.Test.Utilities;
using Xunit;

namespace TestContainers.Integration.Tests.Images.Fixtures
{
    public class DockerfileImageFixture : IAsyncLifetime
    {
        public const string DockerfilePath = "Images/Fixtures/Dockerfiles/Dockerfile";
        public const string DockerfileContextPath = "Images/Fixtures/Dockerfiles/Context";
        public const string DockerfileTransferableFile = "Images/Fixtures/Dockerfiles/Transferables/file1.txt";
        public const string DockerfileTransferableFolder = "Images/Fixtures/Dockerfiles/Transferables/folder1";

        public ImageBuilder<DockerfileImage> ImageBuilder { get; }

        public List<IContainer> ContainersToStop { get; } = new List<IContainer>();

        public List<IImage> ImagesToReap { get; } = new List<IImage>();

        public DockerfileImageFixture()
        {
            ImageBuilder = new ImageBuilder<DockerfileImage>()
                .ConfigureHostConfiguration(builder => builder.AddInMemoryCollection())
                .ConfigureAppConfiguration((context, builder) => builder.AddInMemoryCollection())
                .ConfigureLogging((hostContext, builder) =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Debug);
                });
        }

        public async Task InitializeAsync()
        {
            await Task.WhenAll(ImagesToReap.Select(i => i.ReapAsync()));
        }

        public async Task DisposeAsync()
        {
            // must stop containers before reaping images
            // otherwise images will fail to reap because it's being used by the running container
            await Task.WhenAll(ContainersToStop.Select(c => c.StopAsync()));

            await Task.WhenAll(ImagesToReap.Select(i => i.ReapAsync()));
        }
    }
}
