using Xunit;
using System.Threading.Tasks;
using Npgsql;
using TestContainers.Core.Containers;
using TestContainers.Core.Builders;
using System.Net;
using System.Net.Sockets;

namespace TestContainers.Tests.ContainerTests
{
    public class PostgreSqlFixture : IAsyncLifetime
    {
        public string ConnectionString => Container.ConnectionString;
        PostgreSqlContainer Container { get; }
        
        public PostgreSqlFixture()
        {
            var hostPort = FreeTcpPort();
            
            Container = new DatabaseContainerBuilder<PostgreSqlContainer>()
                .Begin()
                .WithImage($"{PostgreSqlContainer.IMAGE}:{PostgreSqlContainer.DEFAULT_TAG}")
                .WithExposedPorts(PostgreSqlContainer.POSTGRESQL_PORT)
                .WithPortBindings((5432, hostPort))
                .WithEnv(("POSTGRES_PASSWORD", "Password123"))
                .Build();
        }

        public Task InitializeAsync()
        {
            return Container.Start();
        }

        public Task DisposeAsync()
        {
            return Container.Stop();
        }

        private static int FreeTcpPort()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            var port = ((IPEndPoint) l.LocalEndpoint).Port;
            l.Stop();

            return port;
        }
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
