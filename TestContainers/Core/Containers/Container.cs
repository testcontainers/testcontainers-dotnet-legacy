using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Polly;

namespace TestContainers.Core.Containers
{
    public class Container
    {
        readonly DockerClient _dockerClient;
        string _containerId { get; set; }
        public string DockerImageName { get; set; }
        public int[] ExposedPorts { get; set; }
        public (string key, string value)[] EnvironmentVariables { get; set; }
        public (string key, string value)[] Labels { get; set; }
        public ContainerInspectResponse ContainerInspectResponse { get; set; }

        public Container() =>
            _dockerClient = DockerClientFactory.Instance.Client();

        public async Task Start()
        {
            _containerId = await Create();
            await TryStart();
        }

        async Task TryStart()
        {
            await _dockerClient.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());

            await WaitUntilContainerStarted();
        }

        protected virtual async Task WaitUntilContainerStarted()
        {
            var retryUntilContainerStateIsRunning = Policy
                                .HandleResult<ContainerInspectResponse>(c => !c.State.Running)
                                .RetryForeverAsync();

            var containerInspectPolicy = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(1))
                .WrapAsync(retryUntilContainerStateIsRunning)
                .ExecuteAndCaptureAsync(async () => ContainerInspectResponse = await _dockerClient.Containers.InspectContainerAsync(_containerId));

            if (containerInspectPolicy.Outcome == OutcomeType.Failure)
                throw new ContainerLaunchException("Container startup failed", containerInspectPolicy.FinalException);
        }

        async Task<string> Create()
        {
            var progress = new Progress<JSONMessage>();
            await _dockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = DockerImageName,
                    Tag = DockerImageName.Split(':').Last()
                },
                new AuthConfig(),
                progress,
                CancellationToken.None);

            var createContainersParams = ApplyConfiguration();
            var containerCreated = await _dockerClient.Containers.CreateContainerAsync(createContainersParams);
            return containerCreated.ID;
        }

        CreateContainerParameters ApplyConfiguration()
        {
            var exposedPorts = ExposedPorts?.ToList() ?? new List<int>();

            var cfg = new Config
            {
                Image = DockerImageName,
                Env = EnvironmentVariables?.Select(ev => $"{ev.key}={ev.value}").ToList(),
                ExposedPorts = exposedPorts.ToDictionary(e => $"{e}/tcp", e => default(EmptyStruct)),
                Labels = Labels?.ToDictionary(l => l.key, l => l.value),
                Tty = true,
            };

            var portBindings = new Dictionary<string, IList<PortBinding>>();

            exposedPorts.ForEach(e => portBindings.Add($"{e}/tcp", new[] { new PortBinding { HostPort = e.ToString(), HostIP = "" } }));

            return new CreateContainerParameters(cfg)
            {
                HostConfig = new HostConfig
                {
                    PortBindings = portBindings
                }
            };
        }

        public async Task Stop()
        {
            if (string.IsNullOrWhiteSpace(_containerId)) return;

            await _dockerClient.Containers.StopContainerAsync(ContainerInspectResponse.ID, new ContainerStopParameters());
            await _dockerClient.Containers.RemoveContainerAsync(ContainerInspectResponse.ID, new ContainerRemoveParameters());
        }

        public string GetDockerHostIpAddress()
        {
            var dockerHostUri = _dockerClient.Configuration.EndpointBaseUri;

            switch (dockerHostUri.Scheme)
            {
                case "http":
                case "https":
                case "tcp":
                    return dockerHostUri.Host;
                case "npipe": //will have to revisit this for LCOW/WCOW
                case "unix":
                    return File.Exists("/.dockerenv") 
                        ? ContainerInspectResponse.NetworkSettings.Gateway
                        : "localhost";
                default:
                    return null;
            }
        }
    }
}
