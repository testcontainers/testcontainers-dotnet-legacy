using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TestContainers.Containers;
using TestContainers.Core.Builders;
using Xunit;

namespace TestContainers.Tests.Linux
{
    public class CustomizableMySqlFixture : IAsyncLifetime
    {
        const string DB_NAME = "foo";
        const string USER = "bar";
        const string PWD = "baz";

        public MySqlContainer Container { get; }

        public CustomizableMySqlFixture() => Container = new MySqlDatabaseContainerBuilder()
            .Begin()
            .WithImage("mysql:5.5")
            .WithDatabaseName(DB_NAME)
            .WithUserName(USER)
            .WithPassword(PWD)
            .Build();

        public Task InitializeAsync() => Container.Start();

        public Task DisposeAsync() => Container.Stop();
    }

    public class CustomizableMySqlTest : IClassFixture<CustomizableMySqlFixture>
    {
        public MySqlConnection _connection;

        public CustomizableMySqlTest(CustomizableMySqlFixture fixture) => _connection = new MySqlConnection(fixture.Container.ConnectionString);

        [Fact]
        public async Task SimpleTest()
        {
            Console.WriteLine(_connection.ConnectionString);
            const string sqlQuery = "SELECT 1;";
            var result = await performQuery(sqlQuery);

            Assert.Equal((long)1, result);
        }

        async Task<object> performQuery(string sqlQuery)
        {
            await _connection.OpenAsync();
            var cmd = new MySqlCommand(sqlQuery, _connection);
            var reader = (await cmd.ExecuteScalarAsync());
            await _connection.CloseAsync();

            return reader;
        }
    }
}