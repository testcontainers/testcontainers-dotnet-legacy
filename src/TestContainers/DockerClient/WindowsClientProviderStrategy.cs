using System;
using Docker.DotNet;

namespace TestContainers
{
    public class WindowsClientProviderStrategy : DockerClientProviderStrategy
    {
        protected override DockerClientConfiguration Config { get; } =
            new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine"));

        protected override bool IsApplicable() => EnvironmentHelper.IsWindows();

        protected override string GetDescription() => "Docker for Windows (via named pipes)";

    }
}