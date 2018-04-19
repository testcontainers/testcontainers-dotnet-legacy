using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Polly;
using TestContainers.Containers;
using TestContainers.Core.Builders;
using Xunit;

namespace TestContainers.Tests.Windows
{
    public class CustomizableMySqlFixture : IAsyncLifetime
    {
        const string DB_NAME = "foo";
        const string USER = "usuario";
        const string PWD = "Password123";

        public MySqlContainer Container { get; }

        public CustomizableMySqlFixture() => Container = new MySqlDatabaseContainerBuilder()
            .Begin()
            .WithImage("nanoserver/mysql:latest")
            .WithExposedPorts(3306)
            .WithUserName(USER)
            .WithPassword(PWD)
            .Build();

        public Task InitializeAsync() => Container.Start();

        public Task DisposeAsync() => Container.Stop();
    }

    public class CustomizableMySqlTest : IClassFixture<CustomizableMySqlFixture>
    {
        string _connectionString;

        MySqlConnection _connection;

        public CustomizableMySqlTest(CustomizableMySqlFixture fixture) => _connectionString = fixture.Container.ConnectionString;

        [Fact]
        public async Task SimpleTest()
        {
            using (_connection = new MySqlConnection(_connectionString))
            {
                const string sqlQuery = "SELECT 1;";

                await Policy
                  .TimeoutAsync(TimeSpan.FromMinutes(2))
                  .WrapAsync(Policy
                      .Handle<MySqlException>()
                      .WaitAndRetryForeverAsync(iteration => TimeSpan.FromSeconds(10)))
                  .ExecuteAsync(() => _connection.OpenAsync());

                var cmd = new MySqlCommand(sqlQuery, _connection);
                var result = await cmd.ExecuteScalarAsync();

                Assert.Equal((long)1, result);
            }
        }
    }
}