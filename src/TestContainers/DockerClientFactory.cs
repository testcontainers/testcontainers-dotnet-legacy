using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Docker.DotNet;

namespace TestContainers
{
    public sealed class DockerClientFactory
    {
        static volatile DockerClientFactory instance;
        static object syncRoot = new Object();
        DockerClientProviderStrategy strategy { get; } = DockerClientProviderStrategy.GetFirstValidStrategy();
        public static DockerClientFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new DockerClientFactory();
                    }
                }
                return instance;
            }
        }
        public DockerClient Client() => strategy.GetClient();
    }
}
