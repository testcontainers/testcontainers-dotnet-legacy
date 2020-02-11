using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using TestContainers.Images;

namespace TestContainers.Test.Utilities
{
    public static class ImageExtensions
    {
        public static async Task Reap(this IImage image)
        {
            var dockerClient = ((AbstractImage) image).DockerClient;
            var imageName = image.ImageName;

            var images = await dockerClient.Images.ListImagesAsync(new ImagesListParameters());
            var existingImage = images.FirstOrDefault(i => i.RepoTags != null && i.RepoTags.Contains(imageName));
            if (existingImage != null)
            {
                var parameters = new ImageDeleteParameters {Force = true};

                await dockerClient.Images.DeleteImageAsync(imageName, parameters);
            }
        }
    }
}
