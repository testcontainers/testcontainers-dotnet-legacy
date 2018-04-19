using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Docker.DotNet;

namespace TestContainers
{
    public abstract class DockerClientProviderStrategy
    {
        protected abstract DockerClientConfiguration Config { get; }

        protected abstract bool IsApplicable();

        protected abstract string GetDescription();

        public static DockerClientProviderStrategy GetFirstValidStrategy() =>
             AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(t => t.GetTypes())
                .Where(p => p.GetTypeInfo().IsSubclassOf(typeof(DockerClientProviderStrategy)))
                .Select(type => (Activator.CreateInstance(type) as DockerClientProviderStrategy))
                .SingleOrDefault(strategy => strategy.IsApplicable());

        public DockerClient GetClient() => Config.CreateClient();
    }
}

