using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Extensions.Logging;
using TestContainers.Containers.Reaper;
using TestContainers.Internal;
using TestContainers.Internal.GoLang;
using TestContainers.Transferables;

namespace TestContainers.Images
{
    /// <summary>
    /// Represents a docker image that will be built from a Dockerfile
    /// </summary>
    public class DockerfileImage : AbstractImage
    {
        /// <summary>
        /// Default Dockerfile path to be passed into the image build docker command
        /// </summary>
        public const string DefaultDockerfilePath = "Dockerfile";

        /// <summary>
        /// Default .dockerignore path to be used to filter the context from the base path
        /// </summary>
        public const string DefaultDockerIgnorePath = ".dockerignore";

        private static readonly Random Random = new Random();

        /// <summary>
        /// Gets or sets the path to the Dockerfile in the tar archive to be passed into the image build command
        /// </summary>
        public string DockerfilePath { get; set; } = DefaultDockerfilePath;

        /// <summary>
        /// Gets or sets the path to set the base directory for the build context.
        ///
        /// Files ignored by .dockerignore will not be copied into the context.
        /// .dockerignore file must be in the root of the base path
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Indicates whether this image should be deleted after the process ends
        /// </summary>
        public bool DeleteOnExit { get; set; } = true;

        /// <summary>
        /// Transferables that will be passed as build context to the image build command.
        /// Files added by this method will not be filtered by .dockerignore
        /// </summary>
        public IDictionary<string, ITransferable> Transferables { get; } = new Dictionary<string, ITransferable>();

        private readonly ILogger _logger;

        /// <inheritdoc />
        public DockerfileImage(IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(dockerClient, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(GetType());
            ImageName = "testcontainers/" + Random.NextAlphaNumeric(16).ToLower();
        }

        /// <summary>
        /// Runs the docker image build command to build this image
        /// </summary>
        /// <inheritdoc />
        public override async Task<string> Resolve(CancellationToken ct = default)
        {
            if (ct.IsCancellationRequested)
            {
                return null;
            }

            if (DeleteOnExit)
            {
                ResourceReaper.Instance.RegisterImageForCleanup(ImageName, DockerClient);
            }

            _logger.LogDebug("Begin building image: {}", ImageName);

            var tempTarPath = Path.Combine(Path.GetTempPath(), ImageName.Replace('/', '_') + ".tar");

            try
            {
                using (var tempFile = new FileStream(tempTarPath, FileMode.Create))
                using (var tarArchive = TarArchive.CreateOutputTarArchive(tempFile))
                {
                    if (!string.IsNullOrWhiteSpace(BasePath))
                    {
                        // the algorithm here is carefully crafted to minimise the use of
                        // Path.GetFullPath. Path.GetFullPath is used very sparingly and
                        // completely avoided in loops. The reason is because Path.GetFullPath
                        // is a very expensive call and can reduce CPU time by at least 1 order
                        // of magnitude if avoided
                        var fullBasePath = Path.GetFullPath(OS.NormalizePath(BasePath));

                        var ignoreFullPaths = GetIgnores(fullBasePath);

                        // sending a full path will result in entries with full path
                        var allFullPaths = GetAllFilesInDirectory(fullBasePath);

                        // a thread pool that is starved can decrease the performance of
                        // this method dramatically. Using `AsParallel()` will circumvent such issues.
                        // as a result, methods and classes used by this needs to be thread safe.
                        var validFullPaths = allFullPaths
                            .AsParallel()
                            .Where(f => !IsFileIgnored(ignoreFullPaths, f));

                        foreach (var fullPath in validFullPaths)
                        {
                            // we can safely perform a substring without expanding the paths
                            // using Path.GetFullPath because we know fullBasePath has already been
                            // expanded and the paths in validFullPaths are derived from fullBasePath
                            var relativePath = fullPath.Substring(fullBasePath.Length);

                            // if fullBasePath does not end with directory separator,
                            // relativePath will start with directory separator and that should not be the case
                            if (relativePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
                            {
                                relativePath = relativePath.Substring(1);
                            }

                            await new TransferablePath(fullPath)
                                .TransferTo(tarArchive, relativePath, ct)
                                .ConfigureAwait(false);
                        }

                        _logger.LogDebug("Transferred base path [{}] into tar archive", BasePath);
                    }

                    foreach (var entry in Transferables)
                    {
                        var destinationPath = entry.Key;
                        var transferable = entry.Value;
                        await transferable
                            .TransferTo(tarArchive, destinationPath, ct)
                            .ConfigureAwait(false);

                        _logger.LogDebug("Transferred [{}] into tar archive", destinationPath);
                    }

                    tarArchive.Close();
                }

                if (ct.IsCancellationRequested)
                {
                    return null;
                }

                var buildImageParameters = new ImageBuildParameters
                {
                    Dockerfile = DockerfilePath,
                    Labels = DeleteOnExit ? ResourceReaper.Labels : null,
                    Tags = new List<string> {ImageName}
                };

                using (var tempFile = new FileStream(tempTarPath, FileMode.Open))
                {
                    var output =
                        await DockerClient.Images.BuildImageFromDockerfileAsync(tempFile, buildImageParameters, ct);

                    using (var reader = new StreamReader(output))
                    {
                        while (!reader.EndOfStream)
                        {
                            _logger.LogTrace(await reader.ReadLineAsync());
                        }
                    }
                }
            }
            finally
            {
                File.Delete(tempTarPath);
            }

            _logger.LogInformation("Dockerfile image built: {}", ImageName);

            // we should not catch exceptions thrown by inspect because the image is
            // expected to be available since we've just built it
            var image = await DockerClient.Images.InspectImageAsync(ImageName, ct);
            ImageId = image.ID;

            return ImageId;
        }

        private static IList<string> GetIgnores(string fullBasePath)
        {
            var dockerIgnorePath = Path.Combine(fullBasePath, DefaultDockerIgnorePath);
            return File.Exists(dockerIgnorePath)
                ? File.ReadLines(dockerIgnorePath)
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim())
                    .Where(line => !line.StartsWith("#"))
                    .Select(OS.NormalizePath)
                    .Select(line => Path.Combine(fullBasePath, line))
                    .ToList()
                : new List<string>();
        }

        private static bool IsFileIgnored(IEnumerable<string> ignores, string path)
        {
            var matches = new List<string>();
            foreach (var ignore in ignores)
            {
                var goLangPattern = ignore.StartsWith("!") ? ignore.Substring(1) : ignore;
                if (GoLangFileMatch.Match(goLangPattern, path))
                {
                    matches.Add(ignore);
                }
            }

            if (matches.Count <= 0)
            {
                return false;
            }

            var lastMatchingPattern = matches[matches.Count - 1];
            return !lastMatchingPattern.StartsWith("!");
        }

        private static IEnumerable<string> GetAllFilesInDirectory(string directory)
        {
            return Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories);
        }
    }
}
