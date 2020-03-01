using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using TestContainers.Networks;

namespace TestContainers.Test.Utilities
{
    public static class NetworkExtensions
    {
        public static async Task ReapAsync(this INetwork network)
        {
            var dockerClient = ((UserDefinedNetwork) network).DockerClient;
            var networkName = network.NetworkName;

            var networks = await dockerClient.Networks.ListNetworksAsync(new NetworksListParameters());
            var existingNetwork = networks.FirstOrDefault(i => string.Equals(i.Name, networkName));
            if (existingNetwork != null)
            {
                await dockerClient.Networks.DeleteNetworkAsync(existingNetwork.ID);
            }
        }
    }
}
