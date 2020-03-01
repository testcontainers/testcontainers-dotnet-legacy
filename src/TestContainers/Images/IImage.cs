using System.Threading;
using System.Threading.Tasks;

namespace TestContainers.Images
{
    /// <summary>
    /// A docker image
    /// </summary>
    public interface IImage
    {
        /// <summary>
        /// Gets the image name
        /// </summary>
        string ImageName { get; }

        /// <summary>
        /// Gets the image id after it has been built or downloaded
        /// </summary>
        string ImageId { get; }

        /// <summary>
        /// Resolves this image into the local machine
        /// </summary>
        /// <param name="ct">cancellation token</param>
        /// <returns>Task that completes with the ImageId when the image has been resolved locally</returns>
        Task<string> ResolveAsync(CancellationToken ct = default);
    }
}
