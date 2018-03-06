using System;
using System.Runtime.InteropServices;
using Docker.DotNet;

namespace TestContainers
{
    public class UnixSocketClientProviderStrategy : DockerClientProviderStrategy
    {
        protected override DockerClientConfiguration Config { get; } =
            new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock"));

        protected override bool IsApplicable() => Utils.IsOSX() || Utils.IsLinux();

        protected override string GetDescription() =>
            "Docker for windows (via TCP port 2375";

        protected override void Test()
        {
            
        }
    }
}