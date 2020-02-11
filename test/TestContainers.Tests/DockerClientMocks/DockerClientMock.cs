using System;
using Docker.DotNet;
using Moq;

namespace TestContainers.Tests.DockerClientMocks
{
    public class DockerClientMock
    {
        public Mock<IDockerClient> MockDockerClient { get; }

        public Mock<IContainerOperations> MockContainerOperations { get; private set; }

        public DockerClientConfiguration MockClientConfiguration { get; set; }

        public DockerClientMock()
        {
            MockDockerClient = new Mock<IDockerClient>();

            SetupConfiguration();
            SetupContainerOperations();
        }

        private void SetupContainerOperations()
        {
            MockContainerOperations = new Mock<IContainerOperations>();
            MockDockerClient.SetupGet(e => e.Containers).Returns(MockContainerOperations.Object);
        }

        private void SetupConfiguration()
        {
            MockClientConfiguration = new DockerClientConfiguration(new Uri("http://localhost"));
            MockDockerClient.SetupGet(e => e.Configuration).Returns(() => MockClientConfiguration);
        }
    }
}
