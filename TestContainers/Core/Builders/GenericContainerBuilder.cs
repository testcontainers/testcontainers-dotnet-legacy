using System.Collections.Generic;
using System.Linq;
using TestContainers.Core.Containers;

namespace TestContainers.Core.Builders
{
    public abstract class GenericContainerBuilder<TBuilder, TContainer>
        where TBuilder : GenericContainerBuilder<TBuilder, TContainer> 
        where TContainer : GenericContainer, new()
    {
        protected TContainer Container;

        protected readonly TBuilder Self;

        protected GenericContainerBuilder()
        {
            Container = new TContainer();

            Self = (TBuilder)this;
        }

        protected GenericContainerBuilder(string image) : this()
        {
            Container.SetImage(image);
        }

        public TBuilder WithImage(string image)
        {
            Container.SetImage(image);
            return Self;
        }

        public TBuilder WithExposedPort(int port)
        {
            Container.AddExposedPort(port);
            return Self;
        }

        public TBuilder WithExposedPorts(params int[] ports)
        {
            Container.AddExposedPorts(ports);
            return Self;
        }

        public TBuilder WithPortBinding(int hostPort, int containerPort)
        {
            Container.AddPortBinding(hostPort, containerPort);
            return Self;
        }

        public TBuilder WithPortBindings(IEnumerable<KeyValuePair<int, int>> portBindings)
        {
            Container.AddPortBindings(portBindings);
            return Self;
        }

        public TBuilder WithPortBindings(params (int hostPort, int containerPort)[] portBindings)
        {
            Container.AddPortBindings(portBindings.ToDictionary(pb => pb.hostPort, pb => pb.containerPort));
            return Self;
        }

        public TBuilder WithEnv(string key, string value)
        {
            Container.AddEnv(key, value);
            return Self;
        }

        public TBuilder WithEnvs(IEnumerable<KeyValuePair<string, string>> envs)
        {
            Container.AddEnvs(envs);
            return Self;
        }

        public TBuilder WithEnvs(params (string key, string value)[] envs)
        {
            Container.AddEnvs(envs.ToDictionary(e => e.key, e => e.value));
            return Self;
        }

        public TBuilder WithLabel(string key, string value)
        {
            Container.AddLabel(key, value);
            return Self;
        }

        public TBuilder WithLabels(IEnumerable<KeyValuePair<string, string>> labels)
        {
            Container.AddLabels(labels);
            return Self;
        }

        public TBuilder WithLabels(params (string key, string value)[] labels)
        {
            Container.AddLabels(labels.ToDictionary(l => l.key, l => l.value));
            return Self;
        }

        public TBuilder WithMountPoint(string sourcePath, string targetPath, string type)
        {
            Container.AddMountPoint(sourcePath, targetPath, type);
            return Self;
        }

        public TBuilder WithMountPoint(Mount mount)
        {
            Container.AddMountPoint(mount);
            return Self;
        }

        public TBuilder WithMountPoints(params Mount[] mounts)
        {
            Container.AddMountPoint(mounts);
            return Self;
        }

        public TBuilder WithCommand(string cmd)
        {
            Container.SetCommand(cmd);
            return Self;
        }

        public TBuilder WithCommand(params string[] cmds)
        {
            Container.SetCommands(cmds);
            return Self;
        }

        public TContainer Build() => Container;
    }

    public class GenericContainerBuilder<TContainer> : GenericContainerBuilder<GenericContainerBuilder<TContainer>, TContainer>
        where TContainer : GenericContainer, new()
    {
        public GenericContainerBuilder() { }

        public GenericContainerBuilder(string image) : base(image) { }
    }

    public class GenericContainerBuilder : GenericContainerBuilder<GenericContainerBuilder, GenericContainer>
    {
        public GenericContainerBuilder() { }

        public GenericContainerBuilder(string image) : base(image) { }
    }
}
