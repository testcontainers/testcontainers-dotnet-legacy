using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Docker.DotNet;
#if NETSTANDARD2_0
using Microsoft.Extensions.DependencyModel;
#endif

namespace TestContainers
{
    public abstract class DockerClientProviderStrategy
    {
        protected abstract DockerClientConfiguration Config { get; }

        protected abstract bool IsApplicable();

        protected abstract string GetDescription();

        protected abstract void Test();

        public static DockerClientProviderStrategy GetFirstValidStrategy() 
        {
            #if NET45
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            #else 
                var assemblies = 
                    DependencyContext.Default
                    .GetDefaultAssemblyNames()
                    .Select(Assembly.Load);
            #endif


            return assemblies
                .SelectMany(t => t.GetTypes())
                .Where(p => p.GetTypeInfo().IsSubclassOf(typeof(DockerClientProviderStrategy)))
                .Select(type => (Activator.CreateInstance(type) as DockerClientProviderStrategy))
                .SingleOrDefault(strategy => strategy.IsApplicable());
        }
                    
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
    }
    public static class Utils
    {
        public static bool IsWindows() =>
        #if NET45
            true;
        #else
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        #endif

        public static bool IsOSX() =>
        #if NET45
            false;
        #else
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX); 
        #endif

        public static bool IsLinux() =>
        #if NET45
            false;
        #else
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux); 
        #endif
    }
}

