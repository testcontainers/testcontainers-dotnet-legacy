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
        /// <returns>image id when the image is resolved locally</returns>
        Task<string> Resolve(CancellationToken ct = default);
    }
}
