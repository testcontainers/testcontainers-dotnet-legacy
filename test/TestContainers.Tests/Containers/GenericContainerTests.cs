using System;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using Moq;
using TestContainers.Containers;
using TestContainers.Images;
using TestContainers.Tests.DockerClientMocks;
using Xunit;

namespace TestContainers.Tests.Containers
{
    public class GenericContainerTests
    {
        private readonly IContainer _container;
        private readonly DockerClientMock _dockerClientMock;

        public GenericContainerTests()
        {
            _dockerClientMock = new DockerClientMock();
            var dockerClientMock = _dockerClientMock.MockDockerClient;

            var loggerMock = new Mock<ILoggerFactory>();
            var imageMock = new Mock<IImage>();

            _container = new GenericContainer(imageMock.Object, dockerClientMock.Object, loggerMock.Object);
        }

        public class GetDockerHostIpAddress : GenericContainerTests
        {
            [Theory]
            [InlineData("http://www.example.com")]
            [InlineData("https://www.example.com")]
            [InlineData("tcp://1.1.1.1")]
            public void ShouldReturnHttpUrlIfDockerHostIsTcpBased(string url)
            {
                // arrange
                var expected = new Uri(url).Host;
                _dockerClientMock.MockClientConfiguration = new DockerClientConfiguration(new Uri(url));

                // act
                var actual = _container.GetDockerHostIpAddress();

                // assert
                Assert.Equal(expected, actual);
            }

            [Theory]
            [InlineData("npipe://1.1.1.1")]
            [InlineData("unix://1.1.1.1")]
            public void ShouldReturnLocalHostIfDockerHostIsOsSocketBased(string url)
            {
                // arrange
                _dockerClientMock.MockClientConfiguration = new DockerClientConfiguration(new Uri(url));

                // act
                var actual = _container.GetDockerHostIpAddress();

                // assert
                Assert.Equal("localhost", actual);
            }
        }
    }
}
