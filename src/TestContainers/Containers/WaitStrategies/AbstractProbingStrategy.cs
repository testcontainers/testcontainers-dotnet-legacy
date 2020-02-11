using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using JetBrains.Annotations;
using Polly;
using TestContainers.Containers.Exceptions;

namespace TestContainers.Containers.WaitStrategies
{
    /// <summary>
    /// Probes the container regularly to test if services has started
    /// </summary>
    /// <inheritdoc />
    public abstract class AbstractProbingStrategy : IWaitStrategy
    {
        /// <summary>
        /// Timeout before the strategy fails
        /// </summary>
        public TimeSpan Timeout { get; [NotNull] set; } = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Interval between each retry
        /// </summary>
        public TimeSpan RetryInterval { get; [NotNull] set; } = TimeSpan.FromSeconds(3);

        /// <summary>
        /// Exceptions that are considered acceptable in the probe to continue probing
        /// </summary>
        protected abstract IEnumerable<Type> ExceptionTypes { get; }

        /// <inheritdoc />
        public async Task WaitUntil(IDockerClient dockerClient, IContainer container, CancellationToken ct = default)
        {
            var exceptionPolicy = Policy
                .Handle<Exception>(e => ExceptionTypes.Any(t => t.IsInstanceOfType(e)))
                .WaitAndRetryForeverAsync(_ => RetryInterval);

            var result = await Policy
                .TimeoutAsync(Timeout)
                .WrapAsync(exceptionPolicy)
                .ExecuteAndCaptureAsync(async () => { await Probe(dockerClient, container, ct); });

            if (result.Outcome == OutcomeType.Failure)
            {
                throw new ContainerLaunchException(result.FinalException.Message, result.FinalException);
            }
        }

        /// <summary>
        /// The action to probe
        /// </summary>
        /// <param name="dockerClient">Docker client for use</param>
        /// <param name="container">Container to probe</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>A task that completes when the probe completes</returns>
        protected abstract Task Probe(IDockerClient dockerClient, IContainer container, CancellationToken ct = default);
    }
}
