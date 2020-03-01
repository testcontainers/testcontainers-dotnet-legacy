using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using TestContainers.Containers.Reaper.Filters;
using TestContainers.Images;

namespace TestContainers.Containers.Reaper
{
    public class ResourceReaper
    {
        /// <summary>
        /// Class label name applied to containers created by this library
        /// </summary>
        public static readonly string TestContainerLabelName = typeof(System.ComponentModel.IContainer).FullName;

        /// <summary>
        /// Session label added to containers created by this library in this particular run
        /// </summary>
        public static readonly string TestContainerSessionLabelName = TestContainerLabelName + ".SessionId";

        /// <summary>
        /// Session id for this particular run
        /// </summary>
        public static readonly string SessionId = Guid.NewGuid().ToString();

        /// <summary>
        /// Labels that needs to be applied to containers for Ryuk to run properly
        /// </summary>
        public static readonly Dictionary<string, string> Labels = new Dictionary<string, string>
        {
            {TestContainerLabelName, "true"}, {TestContainerSessionLabelName, SessionId}
        };

        public static readonly ResourceReaper Instance = new ResourceReaper();

        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

        private readonly HashSet<string> _imagesToDelete = new HashSet<string>();

        private readonly object _shutdownHookRegisterLock = new object();

        private RyukContainer _ryukContainer;

        private volatile TaskCompletionSource<bool> _ryukStartupTaskCompletionSource;

        private volatile bool _shutdownHookRegistered;

        private ResourceReaper() { }

        /// <summary>
        /// Starts the resource reaper if it is enabled
        /// </summary>
        /// <param name="dockerClient">Docker client to use</param>
        /// <param name="loggerFactory">Optional loggerFactory to log progress</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Task that completes when reaper starts successfully</returns>
        public async Task StartAsync(IDockerClient dockerClient, ILoggerFactory loggerFactory = null,
            CancellationToken ct = default)
        {
            var logger = loggerFactory?.CreateLogger(typeof(ResourceReaper));

            var disabled = Environment.GetEnvironmentVariable("REAPER_DISABLED");
            if ("1".Equals(disabled) || "true".Equals(disabled, StringComparison.InvariantCultureIgnoreCase))
            {
                logger?.LogInformation("Reaper is disabled via $REAPER_DISABLED environment variable");
                return;
            }

            var ryukImage = NullImage.Instance;
            var ryukImageName = Environment.GetEnvironmentVariable("REAPER_IMAGE");
            if (!string.IsNullOrWhiteSpace(ryukImageName))
            {
                ryukImage = new GenericImage(dockerClient, loggerFactory) {ImageName = ryukImageName};
            }

            if (_ryukStartupTaskCompletionSource == null)
            {
                await _initLock.WaitAsync(ct);

                try
                {
                    if (_ryukStartupTaskCompletionSource == null)
                    {
                        logger?.LogDebug("Starting ryuk container ...");

                        _ryukStartupTaskCompletionSource = new TaskCompletionSource<bool>();
                        _ryukContainer = new RyukContainer(ryukImage, dockerClient,
                            loggerFactory ?? NullLoggerFactory.Instance);

                        await _ryukContainer.StartAsync(ct);

                        _ryukContainer.AddToDeathNote(new LabelsFilter(Labels));
                        _ryukStartupTaskCompletionSource.SetResult(true);

                        logger?.LogDebug("Ryuk container started");
                    }
                    else
                    {
                        logger?.LogDebug("Reaper is already started");
                    }
                }
                finally
                {
                    _initLock.Release();
                }
            }
            else
            {
                logger?.LogDebug("Reaper is already started");
            }

            SetupShutdownHook(dockerClient);

            await _ryukStartupTaskCompletionSource.Task;
        }

        /// <summary>
        /// Registers a filter to be cleaned up after this process exits
        /// </summary>
        /// <param name="filter">filter</param>
        public void RegisterFilterForCleanup(IFilter filter)
        {
            _ryukContainer.AddToDeathNote(filter);
        }

        /// <summary>
        /// Registers an image name to be cleaned up when this process exits
        /// </summary>
        /// <param name="imageName">image name to be deleted</param>
        /// <param name="dockerClient">docker client to be used for running the commands in the shutdown hook</param>
        public void RegisterImageForCleanup(string imageName, IDockerClient dockerClient)
        {
            SetupShutdownHook(dockerClient);

            // todo: update ryuk to support image clean up
            // issue: https://github.com/testcontainers/moby-ryuk/issues/6
            _imagesToDelete.Add(imageName);
        }

        internal void KillTcpConnection()
        {
            _ryukContainer?.KillTcpConnection();
        }

        internal void Dispose()
        {
            _ryukContainer?.Dispose();
        }

        internal Task<bool?> IsConnected()
        {
            return _ryukContainer?.IsConnected();
        }

        internal string GetRyukContainerId()
        {
            return _ryukContainer?.ContainerId;
        }

        private void SetupShutdownHook(IDockerClient dockerClient)
        {
            if (_shutdownHookRegistered)
            {
                return;
            }

            lock (_shutdownHookRegisterLock)
            {
                if (_shutdownHookRegistered)
                {
                    return;
                }

                AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => PerformCleanup(dockerClient).Wait();
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    PerformCleanup(dockerClient).Wait();

                    // don't terminate the process immediately, wait for the Main thread to exit gracefully.
                    eventArgs.Cancel = true;
                };

                _shutdownHookRegistered = true;
            }
        }

        private async Task PerformCleanup(IDockerClient dockerClient)
        {
            var imageDeleteParameters = new ImageDeleteParameters
            {
                Force = true,
                // this is actually a badly named variable, it means `noprune` instead of `pleaseprune`
                // this is fixed in https://github.com/microsoft/Docker.DotNet/pull/316 but there hasn't
                // been a release for a very long time (issue still exists in 3.125.2).
                PruneChildren = false
            };

            await Task.WhenAll(
                _imagesToDelete.Select(i => dockerClient.Images.DeleteImageAsync(i, imageDeleteParameters)));
        }
    }
}
