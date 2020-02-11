using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging.Abstractions;
using TestContainers.DockerClient;
using Xunit;

namespace TestContainers.Tests.DockerClient
{
    public class UnixDockerClientProviderTests
    {
        public class GetPriority : UnixDockerClientProviderTests
        {
            [Fact]
            public void ShouldReturn200ForPriority()
            {
                // arrange
                var provider = new UnixDockerClientProvider(NullLogger<UnixDockerClientProvider>.Instance);

                // act
                var result = provider.GetPriority();

                // assert
                Assert.Equal(100, result);
            }
        }

        public class GetConfiguration : UnixDockerClientProviderTests
        {
            [Fact]
            public void ShouldReturnConfigurationWithPresetUnixDockerHost()
            {
                // arrange
                var provider = new UnixDockerClientProvider(NullLogger<UnixDockerClientProvider>.Instance);

                // act
                var result = provider.GetConfiguration();

                // assert
                Assert.Equal(new Uri(UnixDockerClientProvider.UnixSocket), result.EndpointBaseUri);
            }
        }

        public class IsApplicable : UnixDockerClientProviderTests
        {
            [Fact]
            public void ShouldReturnTrueIfEnvironmentIsSetAndOsIsLinuxOrOsx()
            {
                // arrange
                var provider = new UnixDockerClientProvider(NullLogger<UnixDockerClientProvider>.Instance);

                // act
                var result = provider.IsApplicable;

                // assert
                Assert.Equal(
                    RuntimeInformation.IsOSPlatform(OSPlatform.Linux) |
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX),
                    result);
            }
        }
    }
}
