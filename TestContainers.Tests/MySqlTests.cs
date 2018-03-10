using System;
using Xunit;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using TestContainers;
using System.Threading;

namespace TestContainers.Tests
{

    public class MySqlFixture : IAsyncLifetime
    {
        public DbConnection DbConnection { get; private set; }
        Container _container { get; }

        public MySqlFixture() =>
             _container = new ContainerBuilder()
                .Begin()
                .WithImage("nanoserver/mysql:latest")
                .WithExposedPorts(80, 3306)
                //.WithEnv(("MYSQL_ROOT_PASSWORD", "Password123"))
                .Build();

        public async Task InitializeAsync()
        {
            await _container.Start();
            var connectionString = $"Server={_container.IpAddress}; UID=usuario;pwd=Password123;Connect Timeout=30";
            DbConnection = DbConnection.Instance(connectionString);
        }

        public Task DisposeAsync() => _container.Stop();
    }


    public class MySqlTests : IClassFixture<MySqlFixture>
    {
        readonly DbConnection _dbConnection;
        public MySqlTests(MySqlFixture fixture) => _dbConnection = fixture.DbConnection;

        [Fact, Trait("Category", "WCOW")]
        public async Task SimpleTest()
        {
            if (await _dbConnection.IsConnect())
            {
                string query = "SELECT 1;";
                var cmd = new MySqlCommand(query, _dbConnection.Connection);
                var reader = (await cmd.ExecuteScalarAsync());
                Assert.Equal((long)1, reader);
            }
        }
    }
}
