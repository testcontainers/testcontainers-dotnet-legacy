using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;

namespace TestContainers.Containers.StartupStrategies
{
    /// <summary>
    /// Strategy to wait for the container to start
    /// </summary>
    public interface IStartupStrategy
    {
        /// <summary>
        /// Wait for the container to start
        /// </summary>
        /// <param name="dockerClient">Docker client to use</param>
        /// <param name="container">Container to wait for</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Task that completes when the container started successfully</returns>
        Task WaitUntilSuccessAsync(IDockerClient dockerClient, IContainer container, CancellationToken ct = default);
    }
}
