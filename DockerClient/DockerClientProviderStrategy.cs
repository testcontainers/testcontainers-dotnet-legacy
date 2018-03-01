using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Docker.DotNet;
using Microsoft.Extensions.DependencyModel;

public abstract class DockerClientProviderStrategy
{
    protected abstract DockerClientConfiguration Config { get; }

    protected abstract bool IsApplicable();

    protected abstract string GetDescription();

    protected abstract void Test();

    public static DockerClientProviderStrategy GetFirstValidStrategy() => 
        DependencyContext.Default
            .GetDefaultAssemblyNames()
            .Select(Assembly.Load)
            .SelectMany(t => t.GetTypes())
            .Where(p => p.GetTypeInfo().IsSubclassOf(typeof(DockerClientProviderStrategy)))
            .Select(type => (Activator.CreateInstance(type) as DockerClientProviderStrategy))
            .SingleOrDefault(strategy => strategy.IsApplicable());

    public DockerClient GetClient() => Config.CreateClient();

    public string GetDockerHostIpAddress() 
    {
        var dockerHost = Config.EndpointBaseUri;
        switch (dockerHost.Scheme)
        {
            case "http":
            case "https":
            case "tcp":
                return dockerHost.Host;
            case "unix":
                return "localhost";
            default:
                return null;
        }
    }
    public static class Utils
    {
    public static bool IsWindows() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    public static bool IsOSX() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.OSX); 

    public static bool IsLinux() =>
        RuntimeInformation.IsOSPlatform(OSPlatform.Linux); 
    }
        

}

