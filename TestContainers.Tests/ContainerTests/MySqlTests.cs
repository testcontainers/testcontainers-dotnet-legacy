using Xunit;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TestContainers.Core.Containers;
using TestContainers.Core.Builders;

namespace TestContainers.Tests.ContainerTests
{
    public class MySqlFixture : IAsyncLifetime
    {
        public string ConnectionString => Container.GetConnectionString();
        MySqlContainer Container { get; }

        public MySqlFixture() =>
             Container = new DatabaseContainerBuilder<MySqlContainer>()
                .WithImage("mysql:5.7")
                .Build();

        public Task InitializeAsync() => Container.Start();

        public Task DisposeAsync() => Container.Stop();
    }

    public class MySqlTests : IClassFixture<MySqlFixture>
    {
        readonly MySqlConnection _connection;
        public MySqlTests(MySqlFixture fixture) => _connection = new MySqlConnection(fixture.ConnectionString);

        [Fact]
        public async Task SimpleTest()
        {
            string query = "SELECT 1;";
            await _connection.OpenAsync();
            var cmd = new MySqlCommand(query, _connection);
            var reader = await cmd.ExecuteScalarAsync();
            Assert.Equal((long)1, reader);

            await _connection.CloseAsync();
        }
    }
}
