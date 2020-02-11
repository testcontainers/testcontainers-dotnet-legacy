using System;
using System.Runtime.InteropServices;
using Docker.DotNet;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace TestContainers.DockerClient
{
    /// <summary>
    /// Unix domain socket docker client provider
    /// </summary>
    public class UnixDockerClientProvider : AbstractDockerClientProvider
    {
        internal const string UnixSocket = "unix:///var/run/docker.sock";

        /// <summary>
        /// Default provider; default priority
        /// </summary>
        [PublicAPI] public const int Priority = DefaultPriority;

        /// <inheritdoc />
        public override string Description => $"local unix socket: [{UnixSocket}]";

        /// <summary>
        /// Applicable if os is OSX or Linux based
        /// </summary>
        public override bool IsApplicable =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        private readonly DockerClientConfiguration _dockerConfiguration;

        /// <inheritdoc />
        public UnixDockerClientProvider(ILogger<UnixDockerClientProvider> logger)
            : base(logger)
        {
            _dockerConfiguration =
                new DockerClientConfiguration(new Uri(UnixSocket));
        }

        /// <inheritdoc />
        protected override IDockerClient CreateDockerClient()
        {
            return _dockerConfiguration.CreateClient();
        }

        /// <inheritdoc />
        public override int GetPriority()
        {
            return Priority;
        }

        /// <inheritdoc />
        public override DockerClientConfiguration GetConfiguration()
        {
            return _dockerConfiguration;
        }
    }
}
