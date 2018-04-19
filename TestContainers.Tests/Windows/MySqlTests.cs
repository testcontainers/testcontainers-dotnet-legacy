using System;
using Xunit;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TestContainers;
using System.Threading;
using Polly;
using Newtonsoft.Json;
using TestContainers.Core.Containers;
using System.Linq;
using TestContainers.Core.Builders;

namespace TestContainers.Tests.Windows
{
    public class MySqlFixture : IAsyncLifetime
    {
        public MySqlConnection Connection { get; private set; }
        MySqlContainer _container { get; }

        public MySqlFixture() =>
             _container = new MySqlContainerBuilder()
                .Begin()
                .WithImage("nanoserver/mysql:latest")
                .WithExposedPorts(3306)
                .WithUserName("usuario")
                .WithPassword("Password123")
                .Build();

        public async Task InitializeAsync()
        {
            await _container.Start();
            Connection = new MySqlConnection(_container.ConnectionString);

            await Policy
                  .TimeoutAsync(TimeSpan.FromMinutes(2))
                  .WrapAsync(Policy
                      .Handle<MySqlException>()
                      .WaitAndRetryForeverAsync(
                          iteration => TimeSpan.FromSeconds(10)))
                  .ExecuteAsync(() => Connection.OpenAsync());
        }

        public async Task DisposeAsync()
        {
            await Connection.CloseAsync();
            await _container.Stop();
        }

        string GetServerAddress() => _container.ContainerInspectResponse.NetworkSettings.Networks.First().Value.IPAddress;
    }

    public class MySqlTests : IClassFixture<MySqlFixture>
    {
        MySqlConnection _connection { get; }
        public MySqlTests(MySqlFixture fixture) => _connection = fixture.Connection;

        [Fact]
        public async Task SimpleTest()
        {
            string query = "SELECT 1;";
            var cmd = new MySqlCommand(query, _connection);
            var reader = (await cmd.ExecuteScalarAsync());
            Assert.Equal((long)1, reader);
        }
    }
}
