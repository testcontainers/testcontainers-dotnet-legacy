using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using TestContainers.Internal;

namespace TestContainers.Images
{
    /// <summary>
    /// Represents a generic docker image that can be pulled from a docker repository
    /// </summary>
    public class GenericImage : AbstractImage
    {
        private static readonly Striped<SemaphoreSlim> ImagePullLocks = Striped<SemaphoreSlim>.ForSemaphoreSlim();

        private readonly ILogger<GenericImage> _logger;

        /// <inheritdoc />
        public GenericImage(IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(dockerClient, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GenericImage>();
        }

        /// <summary>
        /// Pulls the image from the remote repository if it does not exist locally
        /// </summary>
        /// <inheritdoc />
        public override async Task<string> ResolveAsync(CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                return null;
            }

            if (await CheckIfImageExistsAsync(ct))
            {
                return ImageId;
            }

            // we must only pull an image once
            // the API allows pulling the same image in parallel but suffers from race conditions that produces
            // unpredictable results or errors
            await ImagePullLocks.Get(ImageName).WaitAsync(ct);

            try
            {
                if (!await CheckIfImageExistsAsync(ct))
                {
                    await PullImageAsync(ct);
                }
            }
            finally
            {
                ImagePullLocks.Get(ImageName).Release();
            }

            return ImageId;
        }

        private async Task<bool> CheckIfImageExistsAsync(CancellationToken ct)
        {
            var images = await DockerClient.Images.ListImagesAsync(new ImagesListParameters(), ct);
            var existingImage = images.FirstOrDefault(i => i.RepoTags != null && i.RepoTags.Contains(ImageName));

            if (existingImage == null)
            {
                return false;
            }

            _logger.LogDebug("Image already exists, not pulling: {}", ImageName);
            ImageId = existingImage.ID;
            return true;
        }

        private async Task PullImageAsync(CancellationToken ct)
        {
            _logger.LogInformation("Pulling container image: {}", ImageName);
            var createParameters = new ImagesCreateParameters
            {
                FromImage = ImageName, Tag = ImageName.Split(':').Last(),
            };

            await DockerClient.Images.CreateImageAsync(
                createParameters,
                new AuthConfig(),
                new Progress<JSONMessage>(m =>
                {
                    _logger.LogTrace("[{}] {}", m.Status, m.ProgressMessage);
                }),
                ct);

            // we should not catch exceptions thrown by inspect because the image is
            // expected to be available since we've just pulled it
            var image = await DockerClient.Images.InspectImageAsync(ImageName, ct);
            ImageId = image.ID;
        }
    }
}
