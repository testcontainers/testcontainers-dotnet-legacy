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

        public TBuilder WithPortBinding(int hostPort, int containerPort)
        {
            Container.AddPortBinding(hostPort, containerPort);
            return Self;
        }

        public TBuilder WithEnv(string key, string value)
        {
            Container.AddEnv(key, value);
            return Self;
        }

        public TBuilder WithLabel(string key, string value)
        {
            Container.AddLabel(key, value);
            return Self;
        }

        public TBuilder WithMountPoint(string sourcePath, string targetPath, string type)
        {
            Container.AddMountPoint(sourcePath, targetPath, type);
            return Self;
        }

        public TBuilder WithCommand(string cmd)
        {
            Container.SetCommand(cmd);
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
