using System;
using System.Runtime.InteropServices;
using Docker.DotNet;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace TestContainers.DockerClient
{
    /// <summary>
    /// Npipe socket docker client provider
    /// </summary>
    public class NpipeDockerClientProvider : AbstractDockerClientProvider
    {
        internal const string Npipe = "npipe://./pipe/docker_engine";

        /// <summary>
        /// Default provider; default priority
        /// </summary>
        [PublicAPI] public const int Priority = DefaultPriority;

        /// <inheritdoc />
        public override string Description => $"local npipe: [{Npipe}]";

        /// <summary>
        /// Applicable if os is windows based
        /// </summary>
        public override bool IsApplicable =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        private readonly DockerClientConfiguration _dockerConfiguration;

        /// <inheritdoc />
        public NpipeDockerClientProvider(ILogger<NpipeDockerClientProvider> logger)
            : base(logger)
        {
            _dockerConfiguration =
                new DockerClientConfiguration(new Uri(Npipe));
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
