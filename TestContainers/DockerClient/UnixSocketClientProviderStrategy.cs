using System;
using System.Runtime.InteropServices;
using Docker.DotNet;

namespace TestContainers
{
    public class UnixSocketClientProviderStrategy : DockerClientProviderStrategy
    {
        protected override DockerClientConfiguration Config { get; } =
            new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"));

        protected override bool IsApplicable() => EnvironmentHelper.IsOSX() || EnvironmentHelper.IsLinux();

        protected override string GetDescription() => "Docker for Linux/Mac (via socket)";
    }
}