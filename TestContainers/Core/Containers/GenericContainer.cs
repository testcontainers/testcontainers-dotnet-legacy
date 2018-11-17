using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Docker.DotNet;
using Docker.DotNet.Models;
using Polly;
using TestContainers.Core.Containers.Exceptions;

namespace TestContainers.Core.Containers
{
    public class GenericContainer : IContainer
    {
        public string DockerImageName;
        public ContainerInspectResponse ContainerInfo { get; private set; }

        private string _containerId;
        protected readonly HashSet<int> ExposedPorts = new HashSet<int>();
        protected readonly Dictionary<int, int> PortBindings = new Dictionary<int, int>();
        protected readonly Dictionary<string, string> EnvironmentVariables = new Dictionary<string, string>();
        protected readonly Dictionary<string, string> Labels = new Dictionary<string, string>();
        protected readonly List<Mount> Mounts = new List<Mount>();
        protected string[] CommandParts;

        protected DockerClient DockerClient = DockerClientFactory.Instance.Client();

        public GenericContainer(string dockerImageName)
        {
            DockerImageName = dockerImageName;
        }

        public GenericContainer() : this("alpine:3.5") { }

        public void SetImage(string image)
        {
            DockerImageName = image;
        }

        public void AddExposedPort(int port)
        {
            ExposedPorts.Add(port);
        }

        public void AddExposedPorts(params int[] ports)
        {
            foreach (var port in ports)
            {
                AddExposedPort(port);
            }
        }

        public void AddPortBinding(int hostPort, int containerPort)
        {
            PortBindings[hostPort] = containerPort;
        }

        public void AddPortBindings(IEnumerable<KeyValuePair<int, int>> portBindings)
        {
            foreach (var portBinding in portBindings)
            {
                AddPortBinding(portBinding.Key, portBinding.Value);
            }
        }

        public void AddPortBindings(params (int hostPort, int containerPort)[] portBindings)
        {
            foreach (var (hostPort, containerPort) in portBindings)
            {
                AddPortBinding(hostPort, containerPort);
            }
        }

        public void AddEnv(string key, string value)
        {
            EnvironmentVariables[key] = value;
        }

        public void AddEnvs(IEnumerable<KeyValuePair<string, string>> envs)
        {
            foreach (var env in envs)
            {
                AddEnv(env.Key, env.Value);
            }
        }

        public void AddEnvs(params (string key, string value)[] envs)
        {
            foreach (var (key, value) in envs)
            {
                AddEnv(key, value);
            }
        }

        public void AddLabel(string key, string value)
        {
            Labels[key] = value;
        }

        public void AddLabels(IEnumerable<KeyValuePair<string, string>> labels)
        {
            foreach (var label in labels)
            {
                AddLabel(label.Key, label.Value);
            }
        }

        public void AddLabels(params (string key, string value)[] labels)
        {
            foreach (var (key, value) in labels)
            {
                AddLabel(key, value);
            }
        }

        public void AddMountPoint(Mount mount)
        {
            Mounts.Add(mount);
        }

        public void AddMountPoint(string sourcePath, string targetPath, string type)
        {
            AddMountPoint(new Mount(sourcePath, targetPath, type));
        }

        public void AddMountPoint(params Mount[] mounts)
        {
            foreach (var mount in mounts)
            {
                AddMountPoint(mount);
            }
        }

        public void SetCommand(string cmd)
        {
            CommandParts = cmd.Split(' ');
        }

        public void SetCommands(params string[] cmds)
        {
            CommandParts = cmds;
        }

        public async Task Start()
        {
            _containerId = await Create();
            await TryStart();
        }

        public async Task Stop()
        {
            if (_containerId == null) return;

            await DockerClient.Containers.StopContainerAsync(ContainerInfo.ID, new ContainerStopParameters());
            await DockerClient.Containers.RemoveContainerAsync(ContainerInfo.ID, new ContainerRemoveParameters());
        }

        public async Task ExecuteCommand(params string[] command)
        {
            var containerExecCreateParams = new ContainerExecCreateParameters
            {
                AttachStderr = true,
                AttachStdout = true,
                Cmd = command
            };

            await DockerClient.Containers.ExecCreateContainerAsync(_containerId, containerExecCreateParams);

            await DockerClient.Containers.StartContainerExecAsync(_containerId);
        }

        public string GetDockerHostIpAddress()
        {
            var dockerHostUri = DockerClient.Configuration.EndpointBaseUri;

            switch (dockerHostUri.Scheme)
            {
                case "http":
                case "https":
                case "tcp":
                    return dockerHostUri.Host;
                case "npipe": //will have to revisit this for LCOW/WCOW
                case "unix":
                    return File.Exists("/.dockerenv")
                        ? ContainerInfo.NetworkSettings.Gateway
                        : "localhost";
                default:
                    return null;
            }
        }

        public int GetMappedPort(int originalPort)
        {
            if (_containerId == null)
                throw new ContainerException("Mapped port can only be obtained after the container is started");

            var binding = ContainerInfo?.NetworkSettings.Ports[$"{originalPort}/tcp"];

            if (binding != null && binding.Count > 0 && binding[0] != null)
            {
                return int.Parse(binding[0].HostPort);
            }
            else
            {
                throw new ArgumentException("Requested port (" + originalPort + ") is not mapped");
            }
        }

        protected virtual void Configure() { }

        protected virtual async Task WaitUntilContainerStarted()
        {
            var retryUntilContainerStateIsRunning = Policy.HandleResult<ContainerInspectResponse>(c => !c.State.Running)
                .RetryForeverAsync();

            var containerInspectPolicy = await Policy.TimeoutAsync(TimeSpan.FromMinutes(1))
                .WrapAsync(retryUntilContainerStateIsRunning)
                .ExecuteAndCaptureAsync(async () => await DockerClient.Containers.InspectContainerAsync(_containerId));

            if (containerInspectPolicy.Outcome == OutcomeType.Failure)
                throw new ContainerLaunchException("Container startup failed", containerInspectPolicy.FinalException);

            ContainerInfo = containerInspectPolicy.Result;
        }

        private async Task<string> Create()
        {
            var progress = new Progress<JSONMessage>(async m =>
            {
                Console.WriteLine(m.Status);
                if (m.Error != null)
                    await Console.Error.WriteLineAsync(m.ErrorMessage);
            });

            await DockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = DockerImageName,
                    Tag = DockerImageName.Split(':').Last()
                },
                new AuthConfig(),
                progress,
                CancellationToken.None);
            
            Configure();
            var createContainersParams = ApplyConfiguration();
            var containerCreated = await DockerClient.Containers.CreateContainerAsync(createContainersParams);

            return containerCreated.ID;
        }

        private CreateContainerParameters ApplyConfiguration()
        {
            var cfg = new Config
            {
                Image = DockerImageName,
                Env = EnvironmentVariables.Select(ev => $"{ev.Key}={ev.Value}").ToList(),
                ExposedPorts = ExposedPorts.ToDictionary(e => $"{e}/tcp", e => default(EmptyStruct)),
                Labels = Labels,
                Tty = true,
                Cmd = CommandParts,
                AttachStderr = true,
                AttachStdout = true,
            };

            var portBindings = new Dictionary<string, IList<PortBinding>>();

            foreach (var binding in PortBindings)
            {
                portBindings.Add($"{binding.Key}/tcp", new[] { new PortBinding { HostPort = binding.Value.ToString() } });
            }

            return new CreateContainerParameters(cfg)
            {
                HostConfig = new HostConfig
                {
                    PortBindings = portBindings,
                    Mounts = Mounts
                        .Select(m => new Docker.DotNet.Models.Mount
                            {
                                Source = m.SourcePath,
                                Target = m.TargetPath,
                                Type = m.Type
                            })
                        .ToList(),
                    PublishAllPorts = true
                }
            };
        }

        private async Task TryStart()
        {
            var started = await DockerClient.Containers.StartContainerAsync(_containerId, new ContainerStartParameters());

            if (started)
            {
                await DockerClient.Containers.GetContainerLogsAsync(_containerId, new ContainerLogsParameters
                {
                    ShowStderr = true,
                    ShowStdout = true,
                }); 
            }

            await WaitUntilContainerStarted();
        }
    }
}
