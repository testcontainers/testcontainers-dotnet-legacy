using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using TestContainers.Core.Builders;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.ContainerTests
{
    public class SqlServerFixture : IAsyncLifetime
    {
        public string ConnectionString => Container.ConnectionString;
        SqlServerContainer Container { get; }

        public SqlServerFixture()
        {
            var password = "Password123";
            var builder = new DatabaseContainerBuilder<SqlServerContainer>()
               .Begin()
               .WithExposedPorts(SqlServerContainer.SQLSERVER_PORT);

            if (DockerServerHelper.IsLinux())
            {
                builder
                   .WithImage($"{SqlServerContainer.IMAGE_LNX}:{SqlServerContainer.DEFAULT_TAG}")
                   .WithEnv(("ACCEPT_EULA", "Y"), ("SA_PASSWORD", password));
            }

            if (DockerServerHelper.IsWindows())
            {
                builder
                   .WithImage($"{SqlServerContainer.IMAGE_WIN}:{SqlServerContainer.DEFAULT_TAG}")
                   .WithEnv(("ACCEPT_EULA", "Y"), ("sa_password", password));
            }

            Container = builder.Build();
        }
        public Task InitializeAsync() => Container.Start();

        public Task DisposeAsync() => Container.Stop();
    }

    public class SqlServerTests : IClassFixture<SqlServerFixture>
    {
        readonly SqlConnection _connection;
        public SqlServerTests(SqlServerFixture fixture) => _connection = new SqlConnection(fixture.ConnectionString);

        [Fact]
        public async Task SimpleTest()
        {
            string query = "SELECT 1;";
            await _connection.OpenAsync();
            var cmd = new SqlCommand(query, _connection);
            var reader = (await cmd.ExecuteScalarAsync());
            Assert.Equal(1, reader);

            await _connection.CloseAsync();
        }
    }
}
