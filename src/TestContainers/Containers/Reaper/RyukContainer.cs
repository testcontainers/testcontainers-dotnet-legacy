using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using TestContainers.Containers.Mounts;
using TestContainers.Containers.Reaper.Filters;
using TestContainers.Containers.WaitStrategies;
using TestContainers.Images;
using TestContainers.Internal;

namespace TestContainers.Containers.Reaper
{
    /// <summary>
    /// Container to start ryuk
    /// </summary>
    public class RyukContainer : AbstractContainer
    {
        /// <summary>
        /// Default container image name to use if none is supplied
        /// </summary>
        [PublicAPI] public const string DefaultImageName = "quay.io/testcontainers/ryuk";

        /// <summary>
        /// Default image tag to use if none is supplied
        /// </summary>
        [PublicAPI] public const string DefaultTagName = "0.2.3";

        /// <inheritdoc />
        protected override string DefaultImage { get => DefaultImageName; }

        /// <inheritdoc />
        protected override string DefaultTag { get => DefaultTagName; }

        private const string RyukAck = "ACK";

        private const int RyukPort = 8080;

        private readonly ILogger<RyukContainer> _logger;

        private readonly BatchWorker _sendToRyukWorker;

        private readonly BatchWorker _connectToRyukWorker;

        private readonly List<string> _deathNote = new List<string>();

        private string _ryukHost;

        private int _ryukPort;

        private TcpClient _tcpClient;

        private Stream _tcpWriter;

        private StreamReader _tcpReader;

        /// <inheritdoc />
        public RyukContainer(IImage image, IDockerClient dockerClient, ILoggerFactory loggerFactory)
            : base(image, dockerClient, loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RyukContainer>();
            _sendToRyukWorker = new BatchWorkerFromDelegate(SendToRyukAsync);
            _connectToRyukWorker = new BatchWorkerFromDelegate(ConnectToRyukAsync);
        }

        /// <inheritdoc />
        protected override async Task ConfigureHookAsync(CancellationToken ct = default)
        {
            await base.ConfigureHookAsync(ct);

            WaitStrategy = new ExposedPortsWaitStrategy(new List<int> {RyukPort});
            ExposedPorts.Add(RyukPort);

            BindMounts.Add(new Bind
            {
                // apparently this is the correct way to mount the docker socket on both windows and linux
                // mounting the npipe will not work
                HostPath = "//var/run/docker.sock",
                // ryuk is a linux container, so we have to mount onto the linux socket
                ContainerPath = "/var/run/docker.sock",
                AccessMode = AccessMode.ReadOnly
            });

            AutoRemove = true;
        }

        /// <inheritdoc />
        protected override Task ServiceStartedHookAsync(CancellationToken ct = default)
        {
            _ryukHost = GetDockerHostIpAddress();
            _ryukPort = GetMappedPort(RyukPort);

            _connectToRyukWorker.Notify();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        protected override Task ContainerStoppingHookAsync(CancellationToken ct = default)
        {
            _tcpClient?.Dispose();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Adds a filter for ryuk to kill
        /// </summary>
        /// <param name="filter">filter to add</param>
        public void AddToDeathNote(IFilter filter)
        {
            _deathNote.Add(filter.ToFilterString());
            _sendToRyukWorker.Notify();
        }

        internal void KillTcpConnection()
        {
            _tcpClient?.Dispose();
        }

        internal void Dispose()
        {
            _tcpClient?.Dispose();
            _connectToRyukWorker.Dispose();
        }

        internal async Task<bool?> IsConnectedAsync()
        {
            await _connectToRyukWorker.WaitForCurrentWorkToBeServicedAsync();
            await _sendToRyukWorker.WaitForCurrentWorkToBeServicedAsync();
            return _tcpClient?.Connected;
        }

        private Task ConnectToRyukAsync()
        {
            try
            {
                if (_tcpClient == null || !_tcpClient.Connected)
                {
                    _tcpClient = new TcpClient(_ryukHost, _ryukPort);
                    _tcpWriter = _tcpClient.GetStream();
                    _tcpReader = new StreamReader(new BufferedStream(_tcpClient.GetStream()), Encoding.UTF8);
                    _sendToRyukWorker.Notify();
                }

                _connectToRyukWorker.Notify(DateTime.UtcNow + TimeSpan.FromSeconds(4));
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Disconnected from ryuk. Reconnecting now.");
                _tcpClient = null;
                _connectToRyukWorker.Notify();
            }

            return Task.CompletedTask;
        }

        private async Task SendToRyukAsync()
        {
            if (_deathNote.Count <= 0)
            {
                return;
            }

            var clone = _deathNote.ToList();

            try
            {
                foreach (var filter in clone)
                {
                    var bodyBytes = Encoding.UTF8.GetBytes(filter + "\n");

                    await _tcpWriter.WriteAsync(bodyBytes, 0, bodyBytes.Length);
                    await _tcpWriter.FlushAsync();

                    var response = await _tcpReader.ReadLineAsync();
                    while (response != null && !RyukAck.Equals(response, StringComparison.InvariantCultureIgnoreCase))
                    {
                        response = await _tcpReader.ReadLineAsync();
                    }

                    _deathNote.Remove(filter);
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Disconnected from ryuk while sending. Reconnecting now.");
                _connectToRyukWorker.Notify();
            }
        }
    }
}
