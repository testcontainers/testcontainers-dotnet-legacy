using System;

namespace TestContainers.Core.Containers
{
    public class ContainerLaunchException : Exception
    {
        public ContainerLaunchException(string message) : base(message) { }
        public ContainerLaunchException(string message, Exception exception) : base(message, exception) { }
    }
}