using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using TestContainers.DockerClient;
using Xunit;

namespace TestContainers.Tests.DockerClient
{
    public class EnvironmentDockerClientProviderTests
    {
        public class GetPriority : EnvironmentDockerClientProviderTests
        {
            [Fact]
            public void ShouldReturn200ForPriority()
            {
                // arrange
                var provider = new EnvironmentDockerClientProvider(NullLogger<EnvironmentDockerClientProvider>.Instance);

                // act
                var result = provider.GetPriority();

                // assert
                Assert.Equal(200, result);
            }
        }

        public class TryTest : EnvironmentDockerClientProviderTests
        {
            [Fact]
            public async Task ShouldReturnFalseIfDockerHostNotSet()
            {
                // arrange
                Environment.SetEnvironmentVariable(EnvironmentDockerClientProvider.DockerHostEnvironmentVariable, "");
                var provider = new EnvironmentDockerClientProvider(NullLogger<EnvironmentDockerClientProvider>.Instance);

                // act
                var result = await provider.TryTest();

                // assert
                Assert.False(result);
            }

            [Fact]
            public async Task ShouldTReturnFalseIfDockerHostDoesNotStartWithTcpOrUnix()
            {
                // arrange
                const string mockDockerHostUri = "http://my-mock-docker-host";
                Environment.SetEnvironmentVariable(EnvironmentDockerClientProvider.DockerHostEnvironmentVariable,
                    mockDockerHostUri);
                var provider = new EnvironmentDockerClientProvider(NullLogger<EnvironmentDockerClientProvider>.Instance);

                // act
                var result = await provider.TryTest();

                // assert
                Assert.False(result);
            }

            [Fact]
            public async Task ShouldReturnFalseIfDockerDoesNotExistAtDockerHost()
            {
                // arrange
                const string mockDockerHostUri = "tcp://my-mock-docker-host";
                Environment.SetEnvironmentVariable(EnvironmentDockerClientProvider.DockerHostEnvironmentVariable,
                    mockDockerHostUri);
                var provider = new EnvironmentDockerClientProvider(NullLogger<EnvironmentDockerClientProvider>.Instance);

                // act
                var result = await provider.TryTest();

                // assert
                Assert.False(result);
            }
        }

        public class GetConfiguration : EnvironmentDockerClientProviderTests
        {
            [Fact]
            public void ShouldReturnConfigurationWithDockerHost()
            {
                // arrange
                const string mockDockerHostUri = "tcp://my-mock-docker-host";
                Environment.SetEnvironmentVariable(EnvironmentDockerClientProvider.DockerHostEnvironmentVariable,
                    mockDockerHostUri);
                var provider = new EnvironmentDockerClientProvider(NullLogger<EnvironmentDockerClientProvider>.Instance);

                // act
                var result = provider.GetConfiguration();

                // assert
                Assert.Equal(new Uri(mockDockerHostUri), result.EndpointBaseUri);
            }
        }

        public class IsApplicable : EnvironmentDockerClientProviderTests
        {
            [Fact]
            public void ShouldReturnFalseIfEnvironmentVariableIsNotSet()
            {
                // arrange
                Environment.SetEnvironmentVariable(EnvironmentDockerClientProvider.DockerHostEnvironmentVariable, "");
                var provider = new EnvironmentDockerClientProvider(NullLogger<EnvironmentDockerClientProvider>.Instance);

                // act
                var result = provider.IsApplicable;

                // assert
                Assert.False(result);
            }

            [Fact]
            public void ShouldReturnTrueIfEnvironmentIsSetAndOsIsLinux()
            {
                // arrange
                Environment.SetEnvironmentVariable(EnvironmentDockerClientProvider.DockerHostEnvironmentVariable,
                    "my host");
                var provider = new EnvironmentDockerClientProvider(NullLogger<EnvironmentDockerClientProvider>.Instance);

                // act
                var result = provider.IsApplicable;

                // assert
                Assert.Equal(RuntimeInformation.IsOSPlatform(OSPlatform.Linux), result);
            }

            [Fact]
            public void ShouldReturnTrueIfEnvironmentIsSetAndHostStartsWithTcp()
            {
                // arrange
                Environment.SetEnvironmentVariable(EnvironmentDockerClientProvider.DockerHostEnvironmentVariable,
                    "tcp://my host");
                var provider = new EnvironmentDockerClientProvider(NullLogger<EnvironmentDockerClientProvider>.Instance);

                // act
                var result = provider.IsApplicable;

                // assert
                Assert.True(result);
            }
        }
    }
}
