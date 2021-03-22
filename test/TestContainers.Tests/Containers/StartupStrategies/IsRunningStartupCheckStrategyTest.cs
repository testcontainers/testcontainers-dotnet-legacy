using System;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Moq;
using TestContainers.Containers;
using TestContainers.Containers.Exceptions;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Tests.DockerClientMocks;
using Xunit;

namespace TestContainers.Tests.Containers.StartupStrategies
{
    public class IsRunningStartupCheckStrategyTest
    {
        private readonly IStartupStrategy _strategy;
        private readonly Mock<IContainer> _mockContainer;
        private readonly Mock<IDockerClient> _dockerClientMock;
        private readonly ContainerState _containerStateMock;

        public IsRunningStartupCheckStrategyTest()
        {
            var mockContainerId = "my_container_id";
            _mockContainer = new Mock<IContainer>();
            _mockContainer
                .SetupGet(e => e.ContainerId)
                .Returns(mockContainerId);

            var dockerClientMock = new DockerClientMock();
            _dockerClientMock = dockerClientMock.MockDockerClient;

            var mockInspectResponse = new ContainerInspectResponse();
            dockerClientMock
                .MockContainerOperations
                .Setup(e => e.InspectContainerAsync(mockContainerId, default))
                .Returns(Task.FromResult(mockInspectResponse));

            _containerStateMock = new ContainerState();
            mockInspectResponse.State = _containerStateMock;

            _strategy = new IsRunningStartupCheckStrategy();
        }

        [Fact]
        public async Task ShouldCompleteSuccessfullyIfStateIsRunning()
        {
            // arrange
            _containerStateMock.Running = true;

            // act
            var result = _strategy.WaitUntilSuccessAsync(_dockerClientMock.Object, _mockContainer.Object);
            await result;

            // assert
            Assert.True(result.IsCompletedSuccessfully);
        }

        [Fact]
        public async Task ShouldThrowContainerLaunchExceptionIfContainerStoppedAfterStarting()
        {
            // arrange
            _containerStateMock.FinishedAt = "some finish time";

            // act
            var ex = await Record.ExceptionAsync(async () =>
                await _strategy.WaitUntilSuccessAsync(_dockerClientMock.Object, _mockContainer.Object));

            // assert
            Assert.IsType<ContainerLaunchException>(ex);
            Assert.IsType<InvalidOperationException>(ex.InnerException);
        }

        [Fact]
        public async Task ShouldWaitUntilContainerIsInTheRunningState()
        {
            // arrange
            _containerStateMock.Running = false;
            var task = _strategy.WaitUntilSuccessAsync(_dockerClientMock.Object, _mockContainer.Object);
            Assert.False(task.IsCompleted);

            // act
            _containerStateMock.Running = true;
            await task;

            // assert
            Assert.True(task.IsCompletedSuccessfully);
        }
    }
}
