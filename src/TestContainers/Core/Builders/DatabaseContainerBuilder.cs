using TestContainers.Core.Containers;

namespace TestContainers.Core.Builders
{
    public class DatabaseContainerBuilder<TDatabaseContainer> : ContainerBuilder<TDatabaseContainer, DatabaseContainerBuilder<TDatabaseContainer>>
    where TDatabaseContainer : DatabaseContainer, new()
    {
        public DatabaseContainerBuilder<TDatabaseContainer> WithUserName(string userName)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                container.UserName = userName;
                return container;
            });

            return this;
        }

        public DatabaseContainerBuilder<TDatabaseContainer> WithPassword(string password)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                container.Password = password;
                return container;
            });

            return this;
        }

        public DatabaseContainerBuilder<TDatabaseContainer> WithDatabaseName(string databaseName)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                container.DatabaseName = databaseName;
                return container;
            });

            return this;
        }
    }
}