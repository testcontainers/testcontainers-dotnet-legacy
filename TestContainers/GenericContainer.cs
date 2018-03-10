using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace TestContainers
{
    static class FnUtils
    {
        public static Func<A, C> Compose<A, B, C>(Func<A, B> f1, Func<B, C> f2) =>
            (a) => f2(f1(a));
    }
    public class ContainerBuilder
    {
        Func<Container, Container> fn = null;
        public ContainerBuilder Begin()
        {
            fn = (ignored) => new Container();
            return this;
        }

        public ContainerBuilder WithImage(string dockerImageName)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                container.DockerImageName = dockerImageName;
                return container;
            });

            return this;
        }

        public ContainerBuilder WithExposedPorts(params int[] ports)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                container.ExposedPorts = ports;
                return container;
            });

            return this;
        }

        public ContainerBuilder WithEnv(params (string key, string value)[] keyValuePairs)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                container.EnvironmentVariables = keyValuePairs;
                return container;
            });

            return this;
        }

        public Container Build() =>
            fn(null);
    }

    public class Container
    {
        readonly DockerClient _dockerClient;

        public Container() =>
            _dockerClient = DockerClientFactory.Instance.Client();

        public string DockerImageName { get; set; }

        public int[] ExposedPorts { get; set; }

        public string IpAddress { get; private set; }

        public string ContainerId { get; set; }

        public (string key, string value)[] EnvironmentVariables { get; set; }

        public async Task Start()
        {
            var progress = new Progress<JSONMessage>();
            await _dockerClient.Images.CreateImageAsync(
                new ImagesCreateParameters
                {
                    FromImage = DockerImageName,
                    Tag = DockerImageName.Split(':')[1]
                },
                new AuthConfig(),
                progress,
                CancellationToken.None);

            var exposedPorts = ExposedPorts?.ToList() ?? new List<int>();

            var cfg = new Config
            {
                Image = DockerImageName,
                Env = EnvironmentVariables?.Select(ev => $"{ev.key}={ev.value}").ToList(),
                ExposedPorts = exposedPorts.ToDictionary(e => $"{e}/tcp", e => default(EmptyStruct)),
                Tty = true,
            };

            var portBindings = new Dictionary<string, IList<PortBinding>>();

            exposedPorts.ForEach(e => portBindings.Add($"{e}/tcp", new[] { new PortBinding { HostPort = e.ToString(), HostIP = "" } }));

            var createContainerParams = new CreateContainerParameters(cfg)
            {
                HostConfig = new HostConfig
                {
                    PortBindings = portBindings
                }
            };

            var containerCreated = await _dockerClient.Containers.CreateContainerAsync(createContainerParams);
            await _dockerClient.Containers.StartContainerAsync(containerCreated.ID, new ContainerStartParameters());

            var inspectResult = await _dockerClient.Containers.InspectContainerAsync(containerCreated.ID);
            if (!inspectResult.State.Running)
                throw new Exception("Container is not running");

            IpAddress = inspectResult.NetworkSettings.IPAddress;
            ContainerId = inspectResult.ID;
        }

        public async Task Stop()
        {
            await _dockerClient.Containers.StopContainerAsync(ContainerId, new ContainerStopParameters());

            await _dockerClient.Containers.RemoveContainerAsync(ContainerId, new ContainerRemoveParameters());
        }
    }
}
