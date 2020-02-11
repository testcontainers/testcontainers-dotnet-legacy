using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;

namespace TestContainers.Containers.WaitStrategies
{
    /// <inheritdoc />
    public class ProbingStrategy : AbstractProbingStrategy
    {
        private readonly Func<IDockerClient, IContainer, CancellationToken, Task> _probe;

        /// <inheritdoc />
        protected override IEnumerable<Type> ExceptionTypes { get; }

        /// <inheritdoc />
        public ProbingStrategy(Func<IDockerClient, IContainer, CancellationToken, Task> probe,
            params Type[] exceptionTypes)
        {
            _probe = probe;
            ExceptionTypes = exceptionTypes;
        }

        /// <inheritdoc />
        protected override async Task Probe(IDockerClient dockerClient, IContainer container, CancellationToken ct = default)
        {
            await _probe.Invoke(dockerClient, container, ct);
        }
    }
}
