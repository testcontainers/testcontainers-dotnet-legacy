using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using JetBrains.Annotations;

namespace TestContainers.DockerClient
{
    /// <summary>
    /// Interface for a docker client provider
    /// </summary>
    [PublicAPI]
    public interface IDockerClientProvider
    {
        /// <summary>
        /// Describes the provider and it's properties
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Indicates whether this provider is applicable based on environment
        /// </summary>
        bool IsApplicable { get; }

        /// <summary>
        /// Priority of the provider.
        /// The bigger the number, the higher the priority.
        /// </summary>
        /// <returns>The priority of the provider</returns>
        int GetPriority();

        /// <summary>
        /// Gets the configuration created by this provider
        /// </summary>
        /// <returns>Docker configuration used to create IDockerClients</returns>
        DockerClientConfiguration GetConfiguration();

        /// <summary>
        /// Tests if this provider actually works, if applicable
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>True if this provider could connect to docker</returns>
        Task<bool> TryTestAsync(CancellationToken ct = default);
    }
}
