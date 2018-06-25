using TestContainers.Core.Containers;
namespace TestContainers.Core.Builders
{
    public class MySqlContainerBuilder : DatabaseContainerBuilder<MySqlContainer, MySqlContainerBuilder>
    {
        public override DatabaseContainerBuilder<MySqlContainer, MySqlContainerBuilder> WithDatabaseName(string databaseName)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                container.DatabaseName = databaseName;
                return container;
            });

            return WithEnv(("MYSQL_DATABASE", databaseName));
        }

        public override DatabaseContainerBuilder<MySqlContainer, MySqlContainerBuilder> WithPassword(string password)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                container.Password = password;
                return container;
            });

            return WithEnv(("MYSQL_PASSWORD", password));
        }

        public override DatabaseContainerBuilder<MySqlContainer, MySqlContainerBuilder> WithUserName(string userName)
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                container.UserName = userName;
                return container;
            });

            return WithEnv(("MYSQL_USER", userName));
        }
    }
}