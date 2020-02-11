namespace TestContainers.Containers.Mounts
{
    /// <summary>
    /// Binding for mounts
    /// </summary>
    public class Bind : IBind
    {
        /// <summary>
        /// Absolute path to the folder to mount on the host machine
        /// </summary>
        public string HostPath { get; set; }

        /// <summary>
        /// Absolute path to the folder to mount in the docker container
        /// </summary>
        public string ContainerPath { get; set; }

        /// <summary>
        /// Sets the access mode of the mount
        /// </summary>
        public AccessMode AccessMode { get; set; } = AccessMode.ReadOnly;
    }
}
