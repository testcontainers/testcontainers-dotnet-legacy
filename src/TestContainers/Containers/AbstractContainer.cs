using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using TestContainers.Containers.Exceptions;
using TestContainers.Containers.Mounts;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Images;
using TestContainers.Networks;

namespace TestContainers.Containers
{
    /// <inheritdoc />
    public abstract class AbstractContainer : IContainer
    {
        /// <summary>
        /// Internal hostname to reach a host from inside a container
        /// </summary>
        public const string HostMachineHostname = "host.docker.internal";

        /// <summary>
        /// Http url version of the HostMachineHostName
        /// </summary>
        public const string HostMachineUrl = "http://" + HostMachineHostname;

        private const string TcpExposedPortFormat = "{0}/tcp";

        private readonly ILogger _logger;

        /// <inheritdoc />
        public IImage Image { get; }

        /// <inheritdoc />
        public string ContainerId { get => CreateContainerResponse?.ID; }

        /// <inheritdoc />
        public string ContainerName { get => ContainerInfo?.Name; }

        /// <inheritdoc />
        public IList<int> ExposedPorts { get; } = new List<int>();

        /// <inheritdoc />
        public IDictionary<int, int> PortBindings { get; } = new Dictionary<int, int>();

        /// <inheritdoc />
        public IDictionary<string, string> Env { get; } = new Dictionary<string, string>();

        /// <inheritdoc />
        public IDictionary<string, string> Labels { get; } = new Dictionary<string, string>();

        /// <inheritdoc />
        public IList<IBind> BindMounts { get; } = new List<IBind>();

        /// <inheritdoc />
        public INetwork Network { get; set; }

        /// <inheritdoc />
        public IList<string> NetWorkAliases { get; } = new List<string>();

        /// <inheritdoc />
        public bool IsPrivileged { get; set; }

        /// <inheritdoc />
        public string WorkingDirectory { get; set; }

        /// <inheritdoc />
        public IList<string> Command { get; } = new List<string>();

        /// <inheritdoc />
        public bool AutoRemove { get; set; }

        /// <inheritdoc />
        public IWaitStrategy WaitStrategy { get; set; } = new NoWaitStrategy();

        /// <inheritdoc />
        public IStartupStrategy StartupStrategy { get; set; } = new IsRunningStartupCheckStrategy();

        /// <summary>
        /// Provides the default image name is an overridable instance variable
        /// This value must be available before construct time, and not populated by the constructor
        /// </summary>
        protected abstract string DefaultImage { get; }

        /// <summary>
        /// Provides the default image tag is an overridable instance variable
        /// This value must be available before construct time, and not populated by the constructor
        /// </summary>
        protected abstract string DefaultTag { get; }

        /// <summary>
        /// DockerClient used to perform docker operations
        /// Exposed for unit testing
        /// </summary>
        internal IDockerClient DockerClient { get; }

        private ContainerInspectResponse ContainerInfo { get; set; }

        private CreateContainerResponse CreateContainerResponse { get; set; }

        /// <inheritdoc />
        protected AbstractContainer(IImage image, IDockerClient dockerClient, ILoggerFactory loggerFactory)
        {
            Image = NullImage.IsNullImage(image) ? CreateDefaultImage(dockerClient, loggerFactory) : image;
            DockerClient = dockerClient;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken ct = default)
        {
            if (ContainerId != null)
            {
                return;
            }

            await ConfigureHook(ct);

            await ContainerStartingHook(ct);

            await ResolveImage(ct);

            await ResolveNetwork(ct);

            await CreateContainer(ct);

            await StartContainer(ct);

            await ContainerStartedHook(ct);

            await StartServices(ct);

            await ServiceStartedHook(ct);
        }

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken ct = default)
        {
            if (ContainerId == null)
            {
                return;
            }

            await ContainerStoppingHook(ct);

            await DockerClient.Containers.StopContainerAsync(ContainerId, new ContainerStopParameters(), ct);

            if (!AutoRemove)
            {
                await DockerClient.Containers.RemoveContainerAsync(ContainerId, new ContainerRemoveParameters(), ct);
            }

            await ContainerStoppedHook(ct);
        }

        /// <inheritdoc />
        public string GetDockerHostIpAddress()
        {
            var dockerHostUri = DockerClient.Configuration.EndpointBaseUri;

            switch (dockerHostUri.Scheme)
            {
                case "http":
                case "https":
                case "tcp":
                    return dockerHostUri.Host;
                case "npipe":
                case "unix":
                    return GetContainerGateway() ?? "localhost";
                default:
                    throw new InvalidOperationException("Docker client is using a unsupported transport: " +
                                                        dockerHostUri);
            }
        }

        /// <inheritdoc />
        public int GetMappedPort(int exposedPort)
        {
            if (ContainerInfo == null)
            {
                throw new InvalidOperationException(
                    "Container must be started before mapped ports can be retrieved");
            }

            var tcpExposedPort = string.Format(TcpExposedPortFormat, exposedPort);

            if (ContainerInfo.NetworkSettings.Ports.TryGetValue(tcpExposedPort, out var binding) &&
                binding.Count > 0 &&
                int.TryParse(binding[0].HostPort, out var mappedPort))
            {
                return mappedPort;
            }

            throw new InvalidOperationException($"ExposedPort[{exposedPort}] is not mapped");
        }

        /// <inheritdoc />
        public async Task<(string stdout, string stderr)> ExecuteCommandAsync(IEnumerable<string> command,
            CancellationToken ct = default)
        {
            if (ContainerInfo == null)
            {
                throw new InvalidOperationException(
                    "Container must be started before mapped ports can be retrieved");
            }

            var parameters = new ContainerExecCreateParameters
            {
                AttachStderr = true, AttachStdout = true, Cmd = command.ToArray()
            };

            var response = await DockerClient.Containers.ExecCreateContainerAsync(ContainerId, parameters, ct);

            var stream = await DockerClient.Containers.StartAndAttachContainerExecAsync(response.ID, false, ct);
            return await stream.ReadOutputToEndAsync(default);
        }

        /// <summary>
        /// Configuration hook for inherited containers to implement
        /// </summary>
        [PublicAPI]
        protected virtual Task ConfigureHook(CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook before starting the container
        /// </summary>
        [PublicAPI]
        protected virtual Task ContainerStartingHook(CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook after starting the container
        /// </summary>
        [PublicAPI]
        protected virtual Task ContainerStartedHook(CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook after service in container started
        /// </summary>
        [PublicAPI]
        protected virtual Task ServiceStartedHook(CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook before stopping the container
        /// </summary>
        [PublicAPI]
        protected virtual Task ContainerStoppingHook(CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hook after stopping the container
        /// </summary>
        [PublicAPI]
        protected virtual Task ContainerStoppedHook(CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        private async Task ResolveImage(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            await Image.Resolve(ct);
        }

        private async Task ResolveNetwork(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            if (Network != null)
            {
                await Network.Resolve(ct);
            }
        }

        private async Task CreateContainer(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            _logger.LogDebug("Creating container for image[{}]", Image.ImageName);

            var createParameters = ApplyConfiguration();
            CreateContainerResponse = await DockerClient.Containers.CreateContainerAsync(createParameters, ct);

            _logger.LogDebug("Container created for id[{}] with image[{}]", Image.ImageName, ContainerId);
        }

        private async Task StartContainer(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            try
            {
                _logger.LogDebug("Starting container for id[{}] with image[{}]", ContainerId, Image.ImageName);

                var started =
                    await DockerClient.Containers.StartContainerAsync(ContainerId, new ContainerStartParameters(), ct);
                if (!started)
                {
                    throw new ContainerLaunchException("Unable to start container: " + ContainerId);
                }

                await StartupStrategy.WaitUntilSuccess(DockerClient, this, ct);

                ContainerInfo = await DockerClient.Containers.InspectContainerAsync(ContainerId, ct);

                _logger.LogDebug("Container started for name[{}] with id[{}] and image[{}]", ContainerId,
                    ContainerName, Image.ImageName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to start container with id[{}] and image[{}]", ContainerId,
                    Image.ImageName);

                await PrintContainerLogs(ct);

                throw;
            }
        }

        private async Task StartServices(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }

            try
            {
                _logger.LogDebug("Starting container services for name[{}] with id[{}] and image[{}]", ContainerId,
                    ContainerName, Image.ImageName);

                await WaitStrategy.WaitUntil(DockerClient, this, ct);

                _logger.LogInformation("Container services started for name[{}] with id[{}] and image[{}]", ContainerId,
                    ContainerName, Image.ImageName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to start container services for name[{}] with id[{}] and image[{}]",
                    ContainerId, ContainerName, Image.ImageName);

                await PrintContainerLogs(ct);

                throw;
            }
        }

        private CreateContainerParameters ApplyConfiguration()
        {
            var config = new Config
            {
                Image = Image.ImageName,
                Env = Env.Select(kvp => $"{kvp.Key}={kvp.Value}").ToList(),
                ExposedPorts = ExposedPorts.ToDictionary(
                    e => string.Format(TcpExposedPortFormat, e),
                    e => default(EmptyStruct)),
                Labels = Labels,
                WorkingDir = WorkingDirectory,
                Cmd = Command,
                Tty = true,
                AttachStderr = true,
                AttachStdout = true,
            };

            var hostConfig = new HostConfig
            {
                AutoRemove = AutoRemove,
                NetworkMode = Network?.NetworkName,
                PortBindings = PortBindings.ToDictionary(
                    e => string.Format(TcpExposedPortFormat, e.Key),
                    e => (IList<PortBinding>) new List<PortBinding> {new PortBinding {HostPort = e.Value.ToString()}}),
                Mounts = BindMounts.Select(m => new Mount
                    {
                        Source = m.HostPath,
                        Target = m.ContainerPath,
                        ReadOnly = m.AccessMode == AccessMode.ReadOnly,
                        Type = "bind"
                    })
                    .ToList(),
                PublishAllPorts = true,
                Privileged = IsPrivileged
            };

            var networkConfig = new NetworkingConfig();
            if (Network is UserDefinedNetwork)
            {
                networkConfig.EndpointsConfig = new Dictionary<string, EndpointSettings>
                {
                    {Network.NetworkName, new EndpointSettings {Aliases = NetWorkAliases}}
                };
            }

            return new CreateContainerParameters(config) {HostConfig = hostConfig, NetworkingConfig = networkConfig};
        }

        private IImage CreateDefaultImage(IDockerClient dockerClient, ILoggerFactory loggerFactory)
        {
            return new GenericImage(dockerClient, loggerFactory) {ImageName = $"{DefaultImage}:{DefaultTag}"};
        }

        private string GetContainerGateway()
        {
            if (File.Exists("/.dockerenv") || ContainerInfo == null)
            {
                return null;
            }

            var gateway = ContainerInfo.NetworkSettings.Gateway;
            if (!string.IsNullOrWhiteSpace(gateway))
            {
                return gateway;
            }

            var networkMode = ContainerInfo.HostConfig.NetworkMode;
            if (string.IsNullOrWhiteSpace(networkMode))
            {
                return null;
            }

            if (!ContainerInfo.NetworkSettings.Networks.TryGetValue(networkMode, out var network))
            {
                return null;
            }

            return !string.IsNullOrWhiteSpace(network.Gateway) ? network.Gateway : null;
        }

        private async Task PrintContainerLogs(CancellationToken ct)
        {
            if (ContainerId != null && _logger.IsEnabled(LogLevel.Error))
            {
                using (var logStream = await DockerClient.Containers.GetContainerLogsAsync(ContainerId,
                    new ContainerLogsParameters {ShowStderr = true, ShowStdout = true},
                    ct))
                {
                    using (var reader = new StreamReader(logStream))
                    {
                        _logger.LogError(await reader.ReadToEndAsync());
                    }
                }
            }
        }
    }
}
