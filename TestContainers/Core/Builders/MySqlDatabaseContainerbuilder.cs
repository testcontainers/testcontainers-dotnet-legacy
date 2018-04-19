using TestContainers.Containers;
using TestContainers.Core.Containers;

namespace TestContainers.Core.Builders
{
    public class MySqlDatabaseContainerBuilder : DatabaseContainerBuilder<MySqlContainer>
    {
        public override MySqlContainer Build()
        {
            configure();

            return base.Build();
        }

        MySqlDatabaseContainerBuilder configure()
        {
            fn = FnUtils.Compose(fn, (container) =>
            {
                WithImage(container.DockerImageName);
                WithEnv(("MYSQL_USER", container.UserName), ("MYSQL_PASSWORD", container.Password), ("MYSQL_DATABASE", container.DatabaseName));
                WithExposedPorts(container.ExposedPorts);
                return container;
            });

            return this;
        }
    }
}