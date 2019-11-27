namespace TestContainers.Containers.Mounts
{
    /// <summary>
    /// Binding for mounts
    /// </summary>
    public interface IBind
    {
        /// <summary>
        /// Absolute path to the folder to mount on the host machine
        /// </summary>
        string HostPath { get; }

        /// <summary>
        /// Absolute path to the folder to mount in the docker container
        /// </summary>
        string ContainerPath { get; }

        /// <summary>
        /// Sets the access mode of the mount
        /// </summary>
        AccessMode AccessMode { get; }
    }
}
