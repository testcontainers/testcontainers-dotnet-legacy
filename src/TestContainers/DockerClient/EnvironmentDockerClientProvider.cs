using System;
using System.Runtime.InteropServices;
using Docker.DotNet;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace TestContainers.DockerClient
{
    /// <summary>
    /// Environment variables docker client provider
    /// </summary>
    public class EnvironmentDockerClientProvider : AbstractDockerClientProvider
    {
        internal const string DockerHostEnvironmentVariable = "DOCKER_HOST";

        /// <summary>
        /// Ranks above unix, npipe and other providers
        /// </summary>
        [PublicAPI] public const int Priority = DefaultPriority + 100;

        private readonly string _dockerHost;

        /// <inheritdoc />
        public override string Description => $"environment variable: [{DockerHostEnvironmentVariable}={_dockerHost}]";

        /// <summary>
        /// Applicable if os is OSX or Linux based
        /// </summary>
        public override bool IsApplicable =>
            !string.IsNullOrWhiteSpace(_dockerHost) &&
            (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || _dockerHost.StartsWith("tcp"));

        /// <inheritdoc />
        public EnvironmentDockerClientProvider(ILogger<EnvironmentDockerClientProvider> logger)
            : base(logger)
        {
            _dockerHost = Environment.GetEnvironmentVariable(DockerHostEnvironmentVariable);
        }

        /// <inheritdoc />
        protected override IDockerClient CreateDockerClient()
        {
            var uri = ValidateAndParseDockerHost(_dockerHost);
            return new DockerClientConfiguration(uri).CreateClient();
        }

        /// <inheritdoc />
        public override int GetPriority()
        {
            return Priority;
        }

        /// <inheritdoc />
        public override DockerClientConfiguration GetConfiguration()
        {
            var uri = ValidateAndParseDockerHost(_dockerHost);
            return new DockerClientConfiguration(uri);
        }

        private static Uri ValidateAndParseDockerHost(string dockerHost)
        {
            if (string.IsNullOrWhiteSpace(dockerHost))
            {
                throw new InvalidOperationException($"{DockerHostEnvironmentVariable} is not set");
            }

            var uri = new Uri(dockerHost);
            if (!"tcp".Equals(uri.Scheme) && !"unix".Equals(uri.Scheme))
            {
                throw new InvalidOperationException(
                    $"[{DockerHostEnvironmentVariable}={dockerHost}] only supports tcp and unix uri schemes.");
            }

            return uri;
        }
    }
}
