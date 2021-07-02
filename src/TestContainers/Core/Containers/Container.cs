using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using Polly;

namespace TestContainers.Core.Containers
{
    public class Container
    {
        private const string TcpExposedPortFormat = "{0}/tcp";

        static readonly UTF8Encoding Utf8EncodingWithoutBom = new UTF8Encoding(false);
        readonly DockerClient _dockerClient;
        string _containerId { get; set; }
        public string DockerImageName { get; set; }
        public int[] ExposedPorts { get; set; }
        public (int ExposedPort, int PortBinding)[] PortBindings { get; set; }
        public (string key, string value)[] EnvironmentVariables { get; set; }
        public (string key, string value)[] Labels { get; set; }
        public ContainerInspectResponse ContainerInspectResponse { get; set; }
        public (string SourcePath, string TargetPath, string Type)[] Mounts { get; set; }
        public string[] Commands { get; set; }

        public Container() =>
            _dockerClient = DockerClientFactory.Instance.Client();

        public async Task Start()
        {
            _containerId = await Create();
            await TryStart();
        }

        async Task TryStart()
        {
            var started = await _dockerClient.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());

            if (started)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                using (var logs = await _dockerClient.Containers.GetContainerLogsAsync(_containerId,
                    new ContainerLogsParameters
                    {
                        ShowStderr = true,
                        ShowStdout = true,
                    }, default(CancellationToken)))
#pragma warning restore CS0618 // Type or member is obsolete
                {
                    using (var reader = new StreamReader(logs, Utf8EncodingWithoutBom))
                    {
                        string nextLine;
                        while ((nextLine = await reader.ReadLineAsync()) != null)
                        {
                            Debug.WriteLine(nextLine);
                        }
                    }
                }
            }

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
            var progress = new Progress<JSONMessage>(async (m) =>
            {
                Console.WriteLine(m.Status);
                if (m.Error != null)
                    await Console.Error.WriteLineAsync(m.ErrorMessage);

            });

            var tag = DockerImageName.Split(':').Last();
            var imagesCreateParameters = new ImagesCreateParameters
            {
                FromImage = DockerImageName,
                Tag = tag,
            };

            try
            {
                await this._dockerClient.Images.InspectImageAsync(DockerImageName, CancellationToken.None);
            }
            catch(DockerImageNotFoundException)
            {
                await this._dockerClient.Images.CreateImageAsync(imagesCreateParameters, null, progress, CancellationToken.None);
            }

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
                Cmd = Commands,
                AttachStderr = true,
                AttachStdout = true,
            };

            return new CreateContainerParameters(cfg)
            {
                HostConfig = new HostConfig
                {
                    PortBindings = PortBindings?.ToDictionary(
                        e => string.Format(TcpExposedPortFormat, e.ExposedPort),
                        e => (IList<PortBinding>) new List<PortBinding>
                        {
                            new PortBinding
                            {
                                HostPort = e.PortBinding.ToString()
                            }
                        }),
                    Mounts = Mounts?.Select(m => new Mount
                    {
                        Source = m.SourcePath,
                        Target = m.TargetPath,
                        Type = m.Type,
                    }).ToList(),
                    PublishAllPorts = true,
                }
            };
        }

        public async Task Stop()
        {
            if (string.IsNullOrWhiteSpace(_containerId)) return;

            await _dockerClient.Containers.StopContainerAsync(ContainerInspectResponse.ID, new ContainerStopParameters());
            await _dockerClient.Containers.RemoveContainerAsync(ContainerInspectResponse.ID, new ContainerRemoveParameters());
        }


        public int GetMappedPort(int exposedPort)
        {
            if (ContainerInspectResponse == null)
            {
                throw new InvalidOperationException(
                    "Container must be started before mapped ports can be retrieved");
            }

            var tcpExposedPort = string.Format(TcpExposedPortFormat, exposedPort);

            if (ContainerInspectResponse.NetworkSettings.Ports.TryGetValue(tcpExposedPort, out var binding) &&
                binding.Count > 0 &&
                int.TryParse(binding[0].HostPort, out var mappedPort))
            {
                return mappedPort;
            }

            throw new InvalidOperationException($"ExposedPort[{exposedPort}] is not mapped");
        }

        public async Task ExecuteCommand(params string[] command)
        {
            var containerExecCreateParams = new ContainerExecCreateParameters
            {
                AttachStderr = true,
                AttachStdout = true,
                Cmd = command,
            };

            var response = await _dockerClient.Exec.ExecCreateContainerAsync(_containerId, containerExecCreateParams);

            await _dockerClient.Exec.StartContainerExecAsync(response.ID);
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
