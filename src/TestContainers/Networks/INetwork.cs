using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace TestContainers.Networks
{
    /// <summary>
    /// A docker network
    /// </summary>
    public interface INetwork
    {
        /// <summary>
        /// Gets the network name
        /// </summary>
        string NetworkName { get; }

        /// <summary>
        /// Gets the network id after it has been resolved
        /// </summary>
        string NetworkId { get; }

        /// <summary>
        /// Labels to be set on the network
        /// Dictionary&lt;int key, int value&gt;
        /// </summary>
        [NotNull]
        IDictionary<string, string> Labels { get; }

        /// <summary>
        /// Resolves this network into the local machine
        /// </summary>
        /// <param name="ct">cancellation token</param>
        /// <returns>network id when the network is resolved</returns>
        Task<string> ResolveAsync(CancellationToken ct = default);
    }
}
