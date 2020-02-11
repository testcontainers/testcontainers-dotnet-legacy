using System.IO;
using System.Net;
using System.Threading.Tasks;
using TestContainers.Containers;
using TestContainers.Images;
using TestContainers.Integration.Tests.Images.Fixtures;
using TestContainers.Test.Utilities;
using TestContainers.Transferables;
using Xunit;

namespace TestContainers.Integration.Tests.Images
{
    [Collection(DockerfileImageTestCollection.CollectionName)]
    public class DockerfileImageTests
    {
        private readonly DockerfileImageFixture _fixture;

        private ImageBuilder<DockerfileImage> ImageBuilder => _fixture.ImageBuilder;

        public DockerfileImageTests(DockerfileImageFixture fixture)
        {
            _fixture = fixture;
        }

        public class ResolveTests : DockerfileImageTests
        {
            public ResolveTests(DockerfileImageFixture fixture) : base(fixture)
            {
            }

            [Fact]
            public async Task ShouldResolveImageCorrectly()
            {
                // arrange
                var image = ImageBuilder
                    .ConfigureImage((context, i) =>
                    {
                        i.Transferables["Dockerfile"] = new TransferablePath(DockerfileImageFixture.DockerfilePath);
                    })
                    .Build();

                _fixture.ImagesToReap.Add(image);

                // act
                var actualImageId = await image.Resolve();

                // assert
                var actualImage = await image.DockerClient.Images.InspectImageAsync(image.ImageName);
                Assert.Equal(actualImage.ID, actualImageId);
            }
        }

        public class WithContainer : DockerfileImageTests
        {
            private readonly ContainerBuilder<GenericContainer> _containerBuilder;

            public WithContainer(DockerfileImageFixture fixture) : base(fixture)
            {
                _containerBuilder = new ContainerBuilder<GenericContainer>()
                    .ConfigureContainer((h, c) =>
                    {
                        c.ExposedPorts.Add(80);
                    });
            }

            [Fact]
            public async Task ShouldCreateAndStartContainerSuccessfullyWithRelativeBasePath()
            {
                // arrange
                var image = ImageBuilder
                    .ConfigureImage((context, i) =>
                    {
                        i.BasePath = DockerfileImageFixture.DockerfileContextPath;
                        i.Transferables["Dockerfile"] = new TransferablePath(DockerfileImageFixture.DockerfilePath);
                    })
                    .Build();

                // act
                var host = await StartContainer(image);

                // ignored by .dockerignore
                AssertFileDoesNotExists($"{host}/dummy2.txt");
                AssertFileExists($"{host}/dummy.txt", DockerfileImageFixture.DockerfileContextPath + "/dummy.txt");
            }

            [Fact]
            public async Task ShouldCreateAndStartContainerSuccessfullyWithAbsoluteBasePath()
            {
                // arrange
                var image = ImageBuilder
                    .ConfigureImage((context, i) =>
                    {
                        i.BasePath = Path.GetFullPath(DockerfileImageFixture.DockerfileContextPath);
                        i.Transferables["Dockerfile"] = new TransferablePath(DockerfileImageFixture.DockerfilePath);
                    })
                    .Build();

                // act
                var host = await StartContainer(image);

                // ignored by .dockerignore
                AssertFileDoesNotExists($"{host}/dummy2.txt");
                AssertFileExists($"{host}/dummy.txt", DockerfileImageFixture.DockerfileContextPath + "/dummy.txt");
            }

            [Fact]
            public async Task ShouldCreateAndStartContainerSuccessfullyWithCustomDockerfilePath()
            {
                // arrange
                var image = ImageBuilder
                    .ConfigureImage((context, i) =>
                    {
                        i.BasePath = DockerfileImageFixture.DockerfileContextPath;
                        i.DockerfilePath = "MyDockerfile";
                        i.Transferables["MyDockerfile"] = new TransferablePath(DockerfileImageFixture.DockerfilePath);
                    })
                    .Build();

                // act
                var host = await StartContainer(image);

                // assert
                // ignored by .dockerignore
                AssertFileDoesNotExists($"{host}/dummy2.txt");
                AssertFileExists($"{host}/dummy.txt", DockerfileImageFixture.DockerfileContextPath + "/dummy.txt");
            }

            [Fact]
            public async Task ShouldCreateAndStartContainerSuccessfullyWithTransferables()
            {
                // arrange
                var image = ImageBuilder
                    .ConfigureImage((context, i) =>
                    {
                        i.Transferables["Dockerfile"] = new TransferablePath(DockerfileImageFixture.DockerfilePath);

                        i.Transferables["file1.txt"] =
                            new TransferablePath(DockerfileImageFixture.DockerfileTransferableFile);
                        i.Transferables["folder1"] =
                            new TransferablePath(DockerfileImageFixture.DockerfileTransferableFolder);
                    })
                    .Build();

                // act
                var host = await StartContainer(image);

                // assert
                AssertFileExists($"{host}/file1.txt", DockerfileImageFixture.DockerfileTransferableFile);
                AssertFileExists($"{host}/folder1/file1.txt",
                    DockerfileImageFixture.DockerfileTransferableFolder + "/file1.txt");
            }

            private async Task<string> StartContainer(IImage image)
            {
                var container = _containerBuilder
                    .ConfigureDockerImage(image)
                    .Build();

                _fixture.ContainersToStop.Add(container);
                _fixture.ImagesToReap.Add(image);

                await container.StartAsync();

                var mappedPort = container.GetMappedPort(80);
                return $"http://localhost:{mappedPort}";
            }

            private static void AssertFileExists(string httpPath, string localPath)
            {
                var actual = HttpClientHelper.MakeGetRequest(httpPath);
                var expected = File.ReadAllText(localPath);

                Assert.Equal(expected, actual);
            }

            private static void AssertFileDoesNotExists(string httpPath)
            {
                try
                {
                    HttpClientHelper.MakeGetRequest(httpPath);
                    Assert.True(false);
                }
                catch (WebException e)
                {
                    if (e.Status != WebExceptionStatus.ProtocolError)
                    {
                        throw;
                    }

                    var status = (e.Response as HttpWebResponse)?.StatusCode;
                    Assert.Equal(HttpStatusCode.NotFound, status);
                }
            }
        }
    }
}
