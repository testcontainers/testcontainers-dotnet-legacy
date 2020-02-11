using System.Threading.Tasks;
using Docker.DotNet;
using TestContainers.Containers;
using TestContainers.Images;
using TestContainers.Integration.Tests.Images.Fixtures;
using Xunit;

namespace TestContainers.Integration.Tests.Images
{
    [Collection(GenericImageTestCollection.CollectionName)]
    public class GenericImageTests
    {
        private readonly GenericImageFixture _fixture;

        private IImage Image => _fixture.Image;

        private IDockerClient DockerClient => _fixture.DockerClient;

        public GenericImageTests(GenericImageFixture fixture)
        {
            _fixture = fixture;
        }

        public class ResolveTests : GenericImageTests
        {
            public ResolveTests(GenericImageFixture fixture) : base(fixture)
            {
            }

            [Fact]
            public async Task ShouldResolveImageCorrectly()
            {
                // act
                var actualImageId = await Image.Resolve();

                // assert
                var actualImage = await DockerClient.Images.InspectImageAsync(Image.ImageName);
                Assert.Equal(actualImage.ID, actualImageId);
            }
        }

        public class WithContainer : GenericImageTests
        {
            public WithContainer(GenericImageFixture fixture) : base(fixture)
            {
            }

            [Fact]
            public async Task ShouldCreateAndStartContainerSuccessfully()
            {
                // arrange
                var container = new ContainerBuilder<GenericContainer>()
                    .ConfigureDockerImage(Image)
                    .Build();

                // act
                await container.StartAsync();

                // assert
                Assert.Equal(Image.ImageName, container.DockerImageName);
                await container.StopAsync();
            }
        }
    }
}
