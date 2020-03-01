using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Polly;
using TestContainers.Containers.Exceptions;

namespace TestContainers.Containers.WaitStrategies
{
    /// <summary>
    /// Checks if exposed ports are available to indicate if services have started
    /// </summary>
    /// <inheritdoc />
    public class ExposedPortsWaitStrategy : IWaitStrategy
    {
        /// <summary>
        /// Ports to test
        /// </summary>
        public IList<int> ExposedPorts { get; }

        /// <inheritdoc />
        public ExposedPortsWaitStrategy(IList<int> exposedPorts)
        {
            ExposedPorts = exposedPorts;
        }

        /// <inheritdoc />
        public async Task WaitUntilAsync(IDockerClient dockerClient, IContainer container,
            CancellationToken ct = default)
        {
            var retryPolicy = Policy
                .Handle<SocketException>()
                .WaitAndRetryForeverAsync(retry => TimeSpan.FromSeconds(1));

            var outcome = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(1))
                .WrapAsync(retryPolicy)
                .ExecuteAndCaptureAsync(async () => await AllPortsExposedAsync(container, ct));

            if (outcome.Outcome == OutcomeType.Failure)
            {
                throw new ContainerLaunchException("Container.Abstractions startup failed", outcome.FinalException);
            }
        }

        private Task AllPortsExposedAsync(IContainer container, CancellationToken ct = default)
        {
            var ipAddress = container.GetDockerHostIpAddress();

            foreach (var exposedPort in ExposedPorts)
            {
                if (ct.IsCancellationRequested)
                {
                    return Task.FromCanceled(ct);
                }

                TcpClient client = null;
                try
                {
                    client = new TcpClient(ipAddress, container.GetMappedPort(exposedPort));
                }
                finally
                {
                    client?.Dispose();
                }
            }

            return Task.CompletedTask;
        }
    }
}
