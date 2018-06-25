using Xunit;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TestContainers.Core.Containers;
using TestContainers.Core.Builders;

namespace TestContainers.Tests.ContainerTests
{
    public class MySqlFixture : IAsyncLifetime
    {
        public string RootConnectionString => Container.ConnectionString;

        public string UserConnectionString => $"Server={Container.GetDockerHostIpAddress()};UID={Container.UserName};pwd={Container.Password};SslMode=none;";
        MySqlContainer Container { get; }

        public MySqlFixture() =>
             Container = new MySqlContainerBuilder()
                .Begin()
                .WithImage("mysql:5.7")
                .WithExposedPorts(3306)
                .WithEnv(("MYSQL_ROOT_PASSWORD", "Password123"))
                .WithDatabaseName("testcontainers")
                .WithUserName("tc")
                .WithPassword("password")
                .Build();

        public Task InitializeAsync() => Container.Start();

        public Task DisposeAsync() => Container.Stop();
    }

    public class MySqlTests : IClassFixture<MySqlFixture>
    {
        readonly string _rootConnectionString;
        readonly string _userConnectionString;
        public MySqlTests(MySqlFixture fixture) => (_rootConnectionString, _userConnectionString) = (fixture.RootConnectionString, fixture.UserConnectionString);

        [Fact]
        public async Task RootUserQueryTest()
        {
            string query = "SELECT 1;";
            var connection = new MySqlConnection(_rootConnectionString);
            await connection.OpenAsync();
            var cmd = new MySqlCommand(query, connection);
            var reader = (await cmd.ExecuteScalarAsync());
            Assert.Equal((long)1, reader);

            await connection.CloseAsync();
        }

        [Fact]
        public async Task SpecifiedUserNamePasswordDatabaseQueryTest()
        {
            string query = "SHOW DATABASES LIKE 'testcontainers'";
            var connection = new MySqlConnection(_userConnectionString);
            await connection.OpenAsync();
            var cmd = new MySqlCommand(query, connection);
            var reader = (await cmd.ExecuteScalarAsync());
            Assert.Equal("testcontainers", reader);

            await connection.CloseAsync();
            
        }
    }
}
