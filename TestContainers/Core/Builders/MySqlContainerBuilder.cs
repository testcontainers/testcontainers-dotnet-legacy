using TestContainers.Core.Containers;

namespace TestContainers.Core.Builders
{
    public class MySqlContainerBuilder : DatabaseContainerBuilder<MySqlContainer>
    {
        public override MySqlContainer Build() =>
            //configure();

            base.Build();

        MySqlContainerBuilder configure()
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