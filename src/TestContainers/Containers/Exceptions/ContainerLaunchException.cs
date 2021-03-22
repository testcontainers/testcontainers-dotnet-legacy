using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace TestContainers.Containers.Exceptions
{
    /// <summary>
    /// Exception thrown when a container fails to launch
    /// </summary>
    /// <inheritdoc />
    public class ContainerLaunchException : Exception
    {
        /// <inheritdoc />
        public ContainerLaunchException()
        {
        }

        /// <inheritdoc />
        protected ContainerLaunchException([NotNull] SerializationInfo info, StreamingContext context) : base(info,
            context)
        {
        }

        /// <inheritdoc />
        public ContainerLaunchException(string message) : base(message)
        {
        }

        /// <inheritdoc />
        public ContainerLaunchException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
