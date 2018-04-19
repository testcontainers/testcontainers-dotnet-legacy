using System;
using Xunit;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TestContainers;
using System.Threading;
using Polly;
using TestContainers.Core.Containers;
using System.Linq;
using TestContainers.Containers;
using TestContainers.Core.Builders;

namespace TestContainers.Tests.Linux
{
    public class SimpleMySqlFixture : IAsyncLifetime
    {
        public MySqlContainer Container { get; }

        public SimpleMySqlFixture() =>
            Container = new MySqlDatabaseContainerBuilder()
                .Begin()
                .Build();

        public Task InitializeAsync() => Container.Start();

        public Task DisposeAsync() => Container.Stop();
    }

    public class SimpleMySqlTests : IClassFixture<SimpleMySqlFixture>
    {
        public MySqlConnection _connection;
        public SimpleMySqlTests(SimpleMySqlFixture fixture) => _connection = new MySqlConnection(fixture.Container.ConnectionString);

        [Fact]
        public async Task SimpleTest()
        {
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
