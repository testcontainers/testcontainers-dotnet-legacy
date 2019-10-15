using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using TestContainers.Containers.Mounts;
using TestContainers.Containers.StartupStrategies;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Images;

namespace TestContainers.Containers
{
    /// <summary>
    /// A docker container object
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// Gets the image name
        /// </summary>
        [NotNull]
        string DockerImageName { get; }

        /// <summary>
        /// Gets the docker image
        /// </summary>
        [NotNull]
        IImage DockerImage { get; }

        /// <summary>
        /// Gets the container id after it has started
        /// </summary>
        string ContainerId { get; }

        /// <summary>
        /// List of ports to be exposed on the container
        /// These ports will be automatically mapped to a higher port upon container start
        /// Use <see cref="GetMappedPort"/> to retrieve the automatically mapped port
        /// </summary>
        [NotNull]
        IList<int> ExposedPorts { get; }

        /// <summary>
        /// Port bindings to create for the container. The port must also be exposed by Exposed ports.
        /// Dictionary&lt;int ExposedPort, int PortBinding&gt;
        /// </summary>
        [NotNull]
        IDictionary<int, int> PortBindings { get; }

        /// <summary>
        /// Environment variables to be injected into the container
        /// Dictionary&lt;int key, int value&gt;
        /// </summary>
        [NotNull]
        IDictionary<string, string> Env { get; }

        /// <summary>
        /// Labels to be set on the container
        /// Dictionary&lt;int key, int value&gt;
        /// </summary>
        [NotNull]
        IDictionary<string, string> Labels { get; }

        /// <summary>
        /// List of path bindings between host and container
        /// </summary>
        [NotNull]
        IList<IBind> BindMounts { get; }

        /// <summary>
        /// Sets the container to use privileged mode when this is set
        /// </summary>
        bool IsPrivileged { get; set; }

        /// <summary>
        /// Sets the working directory after the container started
        /// </summary>
        string WorkingDirectory { get; set; }

        /// <summary>
        /// Command to run when the container starts
        /// </summary>
        [NotNull]
        IList<string> Command { get; set; }

        /// <summary>
        /// Option to auto remove the container after use
        /// </summary>
        bool AutoRemove { get; set; }

        /// <summary>
        /// Strategy to use to wait for services in the container to successfully start
        /// </summary>
        IWaitStrategy WaitStrategy { get; }

        /// <summary>
        /// Strategy to use to wait for the container to start
        /// </summary>
        IStartupStrategy StartupStrategy { get; }

        /// <summary>
        /// Starts the container
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>A task that completes when the container fully started</returns>
        Task StartAsync(CancellationToken ct = default);

        /// <summary>
        /// Stops the container
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>A task that completes when the container fully stops</returns>
        Task StopAsync(CancellationToken ct = default);

        /// <summary>
        /// Gets a network host address for this docker instance
        /// </summary>
        /// <returns>The network host for this docker instance</returns>
        /// <exception cref="InvalidOperationException">when docker uses a transport that is not supported</exception>
        string GetDockerHostIpAddress();

        /// <summary>
        /// Gets an mapped port from an exposed port
        /// </summary>
        /// <param name="exposedPort">Exposed port to map</param>
        /// <returns>The mapped port</returns>
        /// <exception cref="InvalidOperationException">when the container has yet to start</exception>
        /// <exception cref="ArgumentException">when the port is not mapped</exception>
        int GetMappedPort(int exposedPort);

        /// <summary>
        /// Executes a command against a running container
        /// </summary>
        /// <param name="command">The command and its parameters to run</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Tuple containing the response of the command</returns>
        /// <exception cref="InvalidOperationException">when the container has yet to start</exception>
        Task<(string stdout, string stderr)> ExecuteCommandAsync(string[] command, CancellationToken ct = default);
    }
}
