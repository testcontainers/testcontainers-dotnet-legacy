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
        /// <param name="containerId">ContainerId to wait for</param>
        /// <returns>Task that completes when the container starts successfully</returns>
        Task WaitUntilSuccess(IDockerClient dockerClient, string containerId);
    }
}
