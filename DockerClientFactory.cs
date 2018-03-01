using System;
using Docker.DotNet;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Linq;
using System.Runtime.InteropServices;

public sealed class DockerClientFactory
{
    bool initialized = false;
    static volatile DockerClientFactory instance;
    static object syncRoot = new Object();

    DockerClientProviderStrategy strategy { get; } = DockerClientProviderStrategy.GetFirstValidStrategy();

    public DockerClientFactory() { }

    public static DockerClientFactory Instance 
    {
        get
        {
            if(instance == null)
            {
                lock(syncRoot)
                {
                    if(instance == null)
                        instance = new DockerClientFactory();
                }
            }
            return instance;
        }
    }
    public DockerClient Client() => strategy.GetClient();
}
