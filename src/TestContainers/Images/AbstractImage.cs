using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace TestContainers.Images
{
    /// <inheritdoc />
    public abstract class AbstractImage : IImage
    {
        /// <inheritdoc />
        public string ImageName { get; [PublicAPI] set; }

        /// <inheritdoc />
        public string ImageId { get; protected set; }

        internal readonly IDockerClient DockerClient;

        private readonly ILogger _logger;

        /// <summary>
        /// Constructs a docker image object
        /// </summary>
        /// <param name="dockerClient">Docker client that will be used to resolve this image</param>
        /// <param name="loggerFactory">Logger to use</param>
        protected AbstractImage(IDockerClient dockerClient, ILoggerFactory loggerFactory)
        {
            DockerClient = dockerClient;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        /// <inheritdoc />
        public abstract Task<string> ResolveAsync(CancellationToken ct = default);
    }
}
