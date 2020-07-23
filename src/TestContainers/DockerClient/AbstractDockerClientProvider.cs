using System;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using Polly;

namespace TestContainers.DockerClient
{
    /// <summary>
    /// Base class for docker client providers
    /// </summary>
    public abstract class AbstractDockerClientProvider : IDockerClientProvider
    {
        private static readonly TimeSpan TestRetryInterval = TimeSpan.FromSeconds(1.5);
        private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        /// The default priority to start with
        /// </summary>
        protected const int DefaultPriority = 100;

        /// <inheritdoc />
        public abstract string Description { get; }

        /// <inheritdoc />
        public abstract bool IsApplicable { get; }

        /// <summary>
        /// Returns a created docker client
        /// </summary>
        /// <returns></returns>
        protected abstract IDockerClient CreateDockerClient();

        /// <inheritdoc />
        public abstract int GetPriority();

        /// <inheritdoc />
        public abstract DockerClientConfiguration GetConfiguration();

        private readonly ILogger _logger;

        /// <summary>
        /// Constructs a docker client provider
        /// </summary>
        /// <param name="logger">Logger to use</param>
        protected AbstractDockerClientProvider(ILogger logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<bool> TryTestAsync(CancellationToken ct = default)
        {
            try
            {
                using (var client = CreateDockerClient())
                {
                    var exceptionPolicy = Policy
                        .Handle<Exception>()
                        .WaitAndRetryForeverAsync(_ => TestRetryInterval);

                    return await Policy
                        .TimeoutAsync(TestTimeout)
                        .WrapAsync(exceptionPolicy)
                        .ExecuteAsync(async () =>
                        {
                            await client.System.PingAsync(ct);
                            return true;
                        });
                }
            }
            catch (Exception e)
            {
                _logger.LogDebug(e, "Test failed!");
                return false;
            }
        }
    }
}
