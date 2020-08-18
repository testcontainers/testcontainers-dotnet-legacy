using System;
using System.Linq;
using Docker.DotNet;
using TestContainers.Core.Containers;

namespace TestContainers.Core.Builders
{
    static class FnUtils
    {
        public static Func<A, C> Compose<A, B, C>(Func<A, B> f1, Func<B, C> f2) =>
            (a) => f2(f1(a));
    }

    public abstract class ContainerBuilder<TContainer, TBuilder>
        where TContainer : Container, new()
        where TBuilder : ContainerBuilder<TContainer, TBuilder>, new()
    {
        protected Func<TContainer, TContainer>
        fn = null;

        public virtual TBuilder Begin()
        {
            fn = (ignored) => new TContainer();
            return (TBuilder) this;
        }

        public TBuilder WithImage(string dockerImageName)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                var tag = dockerImageName.Split(':').Last();
                if (dockerImageName == tag || tag.Contains("/"))
                {
                    dockerImageName = $"{dockerImageName}:latest";
                }
                container.DockerImageName = dockerImageName;
                return container;
            });

            return (TBuilder) this;
        }

        public TBuilder WithExposedPorts(params int[] ports)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                container.ExposedPorts = ports;
                return container;
            });

            return (TBuilder) this;
        }

        public TBuilder WithPortBindings(params (int ExposedPort, int PortBinding)[] portBindings)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                container.PortBindings = portBindings;
                return container;
            });

            return (TBuilder) this;
        }

        public TBuilder WithEnv(params (string key, string value)[] keyValuePairs)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                if (!(container.EnvironmentVariables?.Any() ?? false))
                {
                    container.EnvironmentVariables = keyValuePairs;
                }
                else
                {
                    container.EnvironmentVariables = container.EnvironmentVariables.Union(keyValuePairs).ToArray();
                }

                return container;
            });

            return (TBuilder) this;
        }

        public TBuilder WithLabel(params (string key, string value)[] keyValuePairs)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                container.Labels = keyValuePairs;
                return container;
            });

            return (TBuilder) this;
        }

        public TBuilder WithMountPoints(params (string sourcePath, string targetPath, string type)[] mounts)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                container.Mounts = mounts;
                return container;
            });

            return (TBuilder) this;
        }

        public TBuilder WithCmd(params string[] commands)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                container.Commands = commands;
                return container;
            });

            return (TBuilder) this;
        }

        public virtual TContainer Build() =>
            fn(null);
    }
}
