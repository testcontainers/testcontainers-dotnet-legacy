using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using TestContainers.Containers.Reaper;
using TestContainers.Internal;

namespace TestContainers.Networks
{
    /// <summary>
    /// Represents a user defined network and not one of the built in networks
    /// </summary>
    public class UserDefinedNetwork : INetwork
    {
        private static readonly Random Random = new Random();

        private static readonly Striped<SemaphoreSlim> NetworkCreateLocks = Striped<SemaphoreSlim>.ForSemaphoreSlim();

        /// <inheritdoc />
        public string NetworkId { get; private set; }

        /// <inheritdoc />
        public string NetworkName { get; [PublicAPI] set; }

        /// <inheritdoc />
        public IDictionary<string, string> Labels { get; } = new Dictionary<string, string>();

        internal readonly IDockerClient DockerClient;

        private readonly ILogger<UserDefinedNetwork> _logger;

        private readonly ILoggerFactory _loggerFactory;

        /// <inheritdoc />
        public UserDefinedNetwork(IDockerClient dockerClient, ILoggerFactory loggerFactory)
        {
            DockerClient = dockerClient;
            _logger = loggerFactory.CreateLogger<UserDefinedNetwork>();
            _loggerFactory = loggerFactory;

            NetworkName = "testcontainers/" + Random.NextAlphaNumeric(16).ToLower();
        }

        /// <inheritdoc />
        public async Task<string> Resolve(CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                return null;
            }

            if (await CheckIfNetworkExists(ct))
            {
                return NetworkId;
            }

            // we must only create a single network name once
            // the API allows creating multiple networks with the same name
            // and when this name is referenced in a container, the command will fail complaining
            // that there are more than 1 networks that matches the given name
            await NetworkCreateLocks.Get(NetworkName).WaitAsync(ct);

            try
            {
                if (!await CheckIfNetworkExists(ct))
                {
                    await CreateNetwork(ct);
                }
            }
            finally
            {
                NetworkCreateLocks.Get(NetworkName).Release();
            }

            return NetworkId;
        }

        private async Task<bool> CheckIfNetworkExists(CancellationToken ct)
        {
            var networks = await DockerClient.Networks.ListNetworksAsync(new NetworksListParameters(), ct);
            var existingNetwork = networks.FirstOrDefault(i => string.Equals(i.Name, NetworkName));

            if (existingNetwork == null)
            {
                return false;
            }

            _logger.LogDebug("Network already exists, not creating: {}", NetworkName);
            NetworkId = existingNetwork.ID;
            return true;
        }

        private async Task CreateNetwork(CancellationToken ct)
        {
            _logger.LogInformation("Creating network: {}", NetworkName);

            _logger.LogDebug("Starting reaper ...");
            await ResourceReaper.Instance.StartAsync(DockerClient, _loggerFactory, ct);

            _logger.LogDebug("Adding session labels to network: {}", ResourceReaper.SessionId);
            foreach (var label in ResourceReaper.Labels)
            {
                Labels.Add(label.Key, label.Value);
            }

            var response = await DockerClient.Networks.CreateNetworkAsync(
                new NetworksCreateParameters {Name = NetworkName, CheckDuplicate = true, Labels = Labels}, ct);

            NetworkId = response.ID;
        }
    }
}
