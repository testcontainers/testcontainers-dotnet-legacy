using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging.Abstractions;
using TestContainers.DockerClient;
using Xunit;

namespace TestContainers.Tests.DockerClient
{
    public class DockerClientFactoryTests
    {
        [Fact]
        public async Task ShouldReturnConfigurationThatIsApplicableAndHighestPriority()
        {
            // arrange
            var mockConfig = new DockerClientConfiguration(new Uri("tcp://abcd"));

            // only 1 provider should return the mock config
            var provider1 = new MockDockerClientProvider(false, 1000, () => null, ct => Task.FromResult(true));
            var provider2 = new MockDockerClientProvider(true, 1000, () => mockConfig, ct => Task.FromResult(true));
            var provider3 = new MockDockerClientProvider(true, 100, () => null, ct => Task.FromResult(true));
            var provider4 = new MockDockerClientProvider(true, 20000, () => null, ct => Task.FromResult(false));

            var factory = new DockerClientFactory(new NullLogger<DockerClientFactory>(),
                new List<IDockerClientProvider> {provider1, provider2, provider3, provider4});

            // act
            var result = await factory.Create();

            // assert
            Assert.Equal(mockConfig, result.Configuration);
        }
    }

    internal class MockDockerClientProvider : IDockerClientProvider
    {
        private readonly int _priority;

        private readonly Func<DockerClientConfiguration> _getConfigurationFunction;
        private readonly Func<CancellationToken, Task<bool>> _tryTestFunction;

        public string Description { get; }

        public bool IsApplicable { get; }

        public MockDockerClientProvider(bool isApplicable, int priority,
            Func<DockerClientConfiguration> getConfigurationFunction,
            Func<CancellationToken, Task<bool>> tryTestFunction)
        {
            Description = "";
            IsApplicable = isApplicable;
            _priority = priority;
            _getConfigurationFunction = getConfigurationFunction;
            _tryTestFunction = tryTestFunction;
        }

        public int GetPriority()
        {
            return _priority;
        }

        public DockerClientConfiguration GetConfiguration()
        {
            return _getConfigurationFunction();
        }

        public Task<bool> TryTest(CancellationToken ct = default(CancellationToken))
        {
            return _tryTestFunction(ct);
        }
    }
}
