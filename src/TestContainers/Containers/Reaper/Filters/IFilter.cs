namespace TestContainers.Containers.Reaper.Filters
{
    /// <summary>
    /// Docker filter to find containers
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// The actual filter string passed to the API
        /// </summary>
        /// <returns>A string to be passed to the filters parameter in the Docker API</returns>
        string ToFilterString();
    }
}
