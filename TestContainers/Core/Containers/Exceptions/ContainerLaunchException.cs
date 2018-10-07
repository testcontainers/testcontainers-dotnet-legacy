using System;

namespace TestContainers.Core.Containers.Exceptions
{
    public class ContainerLaunchException : ContainerException
    {
        public ContainerLaunchException(string message) : base(message) { }
        public ContainerLaunchException(string message, Exception exception) : base(message, exception) { }
    }
}