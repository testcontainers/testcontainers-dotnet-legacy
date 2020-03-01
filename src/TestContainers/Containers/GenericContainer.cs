using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using TestContainers.Containers.Reaper;
using TestContainers.Images;

namespace TestContainers.Containers
{
    /// <summary>
    /// Generic implementation for a container. This can be used to start any container.
    /// </summary>
    /// <inheritdoc />
    public class GenericContainer : AbstractContainer
    {
        /// <summary>
        /// Default container image name to use if none is supplied
        /// </summary>
        public const string DefaultImageName = "alpine";

        /// <summary>
        /// Default image tag to use if none is supplied
        /// </summary>
        public const string DefaultTagName = "3.5";

        /// <inheritdoc />
        protected override string DefaultImage => DefaultImageName;

        /// <inheritdoc />
        protected override string DefaultTag => DefaultTagName;

        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        /// <inheritdoc />
        public GenericContainer(IImage image, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(image, dockerClient, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _loggerFactory = loggerFactory;
        }

        /// <inheritdoc />
        protected override async Task ConfigureHook(CancellationToken ct = default)
        {
            await base.ConfigureHook(ct);

            _logger.LogDebug("Adding session labels to generic container: {}", ResourceReaper.SessionId);

            foreach (var label in ResourceReaper.Labels)
            {
                Labels.Add(label.Key, label.Value);
            }
        }

        /// <inheritdoc />
        protected override async Task ContainerStartingHook(CancellationToken ct = default)
        {
            await base.ContainerStartingHook(ct);

            _logger.LogDebug("Starting reaper ...");
            await ResourceReaper.Instance.StartAsync(DockerClient, _loggerFactory, ct);
        }
    }
}
