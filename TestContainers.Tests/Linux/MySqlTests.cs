using System;
using Xunit;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TestContainers;
using System.Threading;
using Polly;
using TestContainers.Core.Containers;
using System.Linq;
using TestContainers.Core.Builders;

namespace TestContainers.Tests.Linux
{
    public class MySqlFixture : IAsyncLifetime
    {
        public MySqlContainer Container { get; }

        public MySqlFixture() =>
             Container = new MySqlContainerBuilder()
                .Begin()
                .WithImage("mysql:latest")
                .WithExposedPorts(3306)
                .WithEnv(("MYSQL_ROOT_PASSWORD", "Password123"))
                .Build();

        public Task InitializeAsync() => Container.Start();

        public Task DisposeAsync() => Container.Stop();
    }

    public class MySqlTests : IClassFixture<MySqlFixture>
    {
        MySqlConnection _connection { get; }
        public MySqlTests(MySqlFixture fixture) => _connection = new MySqlConnection(fixture.Container.ConnectionString);

        [Fact]
        public async Task SimpleTest()
        {
            string query = "SELECT 1;";
            await _connection.OpenAsync();
            var cmd = new MySqlCommand(query, _connection);
            var reader = (await cmd.ExecuteScalarAsync());
            Assert.Equal((long)1, reader);

            await _connection.CloseAsync();
        }
    }
}
