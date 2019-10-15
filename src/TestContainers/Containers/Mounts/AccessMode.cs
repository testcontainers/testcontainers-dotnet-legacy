namespace TestContainers.Containers.Mounts
{
    /// <summary>
    /// Different access modes in bind mounting
    /// </summary>
    public class AccessMode
    {
        /// <summary>
        /// ReadOnly access mode. AKA "ro"
        /// </summary>
        public static readonly AccessMode ReadOnly = new AccessMode("ro");

        /// <summary>
        /// ReadWrite access mode. AKA "rw"
        /// </summary>
        public static readonly AccessMode ReadWrite = new AccessMode("rw");

        /// <summary>
        /// String representation of the mode for docker client consumption
        /// </summary>
        public string Value { get; }

        private AccessMode(string value)
        {
            Value = value;
        }
    }
}
