using TestContainers.Core.Containers;

namespace TestContainers.Core.Builders
{
    public abstract class DatabaseContainerBuilder<TDatabaseContainer, TDatabaseContainerBuilder> : ContainerBuilder<TDatabaseContainer, TDatabaseContainerBuilder>
    where TDatabaseContainer : DatabaseContainer, new()
    where TDatabaseContainerBuilder : ContainerBuilder<TDatabaseContainer, TDatabaseContainerBuilder>, new()

    {
        public abstract DatabaseContainerBuilder<TDatabaseContainer, TDatabaseContainerBuilder> WithUserName(string userName);
        public abstract DatabaseContainerBuilder<TDatabaseContainer, TDatabaseContainerBuilder> WithPassword(string password);
        public abstract DatabaseContainerBuilder<TDatabaseContainer, TDatabaseContainerBuilder> WithDatabaseName(string databaseName);
    }
}