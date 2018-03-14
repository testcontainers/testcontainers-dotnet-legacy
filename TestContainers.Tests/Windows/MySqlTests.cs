using System;
using Xunit;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TestContainers;
using System.Threading;

namespace TestContainers.Tests.Windows
{
    public class MySqlFixture : IAsyncLifetime
    {
        Container _container { get; }

        public MySqlFixture() =>
             _container = new ContainerBuilder()
                .Begin()
                .WithImage("nanoserver/mysql:latest")
                .WithExposedPorts(3306)
                .Build();

        public Task InitializeAsync() => _container.Start();

        public Task DisposeAsync() => _container.Stop();

        public string GetServerAddress() => _container.IpAddress;
    }

    public class MySqlTests : IClassFixture<MySqlFixture>
    {
        string _serverAddress;
        public MySqlTests(MySqlFixture fixture) =>
            _serverAddress = fixture.GetServerAddress();

        [Fact]
        public async Task SimpleTest()
        {
            var connectionString = $"Server=localhost; UID=usuario;pwd=Password123;Connect Timeout=30";
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT 1;";
                var cmd = new MySqlCommand(query, connection);
                var reader = await cmd.ExecuteScalarAsync();
                Assert.Equal((long)1, reader);
            }
        }
    }
}
