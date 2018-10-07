using System;

namespace TestContainers.Core.Containers.Exceptions
{
    public class ContainerException : Exception
    {
        public ContainerException(string message) : base(message) { }
        public ContainerException(string message, Exception exception) : base(message, exception) { }
    }
}
