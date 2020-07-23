using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;

namespace TestContainers.Containers.WaitStrategies
{
    /// <summary>
    /// Strategy for waiting for services in the container to start
    /// </summary>
    public interface IWaitStrategy
    {
        /// <summary>
        /// Wait for the services to start
        /// </summary>
        /// <param name="dockerClient">Docker client to use</param>
        /// <param name="container">Container to wait for</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Task that completes when the services started successfully</returns>
        Task WaitUntilAsync(IDockerClient dockerClient, IContainer container, CancellationToken ct = default);
    }
}
