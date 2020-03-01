using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

namespace TestContainers.DockerClient
{
    /// <summary>
    /// Factory to provide docker clients based on testing different providers
    /// </summary>
    public class DockerClientFactory
    {
        private readonly ILogger<DockerClientFactory> _logger;
        private readonly AsyncLazy<DockerClientConfiguration> _configuration;

        /// <inheritdoc />
        public DockerClientFactory(ILogger<DockerClientFactory> logger,
            IEnumerable<IDockerClientProvider> dockerClientProviders)
        {
            _logger = logger;
            _configuration = new AsyncLazy<DockerClientConfiguration>(async () =>
            {
                var orderedApplicableProviders = dockerClientProviders
                    .OrderByDescending(p => p.GetPriority())
                    .Where(p => p.IsApplicable);

                foreach (var provider in orderedApplicableProviders)
                {
                    var name = provider.GetType().Name;
                    var description = provider.Description;

                    _logger.LogDebug("Testing provider: {}", name);
                    if (await provider.TryTestAsync())
                    {
                        _logger.LogDebug("Provider[{}] found\n{}", name, description);
                        return provider.GetConfiguration();
                    }

                    _logger.LogDebug("Provider[{}] test failed\n{}", name, description);
                }

                throw new InvalidOperationException("There are no supported docker client providers!");
            });
        }

        /// <summary>
        /// Creates a new DockerClient
        /// </summary>
        /// <returns>A task that completes when the client is created</returns>
        public async Task<IDockerClient> CreateAsync(CancellationToken ct = default)
        {
            var configuration = await _configuration.GetValueAsync(ct);
            return configuration.CreateClient();
        }
    }
}
