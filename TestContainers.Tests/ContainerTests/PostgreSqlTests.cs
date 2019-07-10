using Xunit;
using System.Threading.Tasks;
using Npgsql;
using TestContainers.Core.Containers;
using TestContainers.Core.Builders;

namespace TestContainers.Tests.ContainerTests
{
    public class PostgreSqlFixture : IAsyncLifetime
    {
        public string ConnectionString => Container.ConnectionString;
        PostgreSqlContainer Container { get; }

        public PostgreSqlFixture() =>
             Container = new DatabaseContainerBuilder<PostgreSqlContainer>()
                .Begin()
                .WithImage($"{PostgreSqlContainer.IMAGE}:{PostgreSqlContainer.DEFAULT_TAG}")
                .WithExposedPorts(PostgreSqlContainer.POSTGRESQL_PORT)
                .WithPortBindings((PostgreSqlContainer.POSTGRESQL_PORT, PostgreSqlContainer.POSTGRESQL_PORT))
                .WithEnv(("POSTGRES_PASSWORD", "Password123"))
                .Build();

        public Task InitializeAsync() => Container.Start();

        public Task DisposeAsync() => Container.Stop();
    }

    public class PostgreSqlTests : IClassFixture<PostgreSqlFixture>
    {
        readonly NpgsqlConnection _connection;
        public PostgreSqlTests(PostgreSqlFixture fixture) => _connection = new NpgsqlConnection(fixture.ConnectionString);

        [Fact]
        public async Task SimpleTest()
        {
            const string query = "SELECT 1;";
            await _connection.OpenAsync();
            var cmd = new NpgsqlCommand(query, _connection);
            var reader = (await cmd.ExecuteScalarAsync());
            Assert.Equal(1, reader);

            _connection.Close();
        }
    }
}