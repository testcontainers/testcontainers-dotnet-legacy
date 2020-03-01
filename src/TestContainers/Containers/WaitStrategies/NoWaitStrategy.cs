using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;

namespace TestContainers.Containers.WaitStrategies
{
    /// <summary>
    /// Don't wait
    /// </summary>
    /// <inheritdoc />
    public class NoWaitStrategy : IWaitStrategy
    {
        /// <inheritdoc />
        public Task WaitUntilAsync(IDockerClient dockerClient, IContainer container, CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }
}
