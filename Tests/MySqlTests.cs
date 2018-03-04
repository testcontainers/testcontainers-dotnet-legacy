using System;
using Xunit;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

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
                .WithExposedPorts(3306)
                .Build();   
        
        public async Task InitializeAsync() 
        {
            await _container.Start();
            DbConnection = DbConnection.Instance();
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
            if (_dbConnection.IsConnect())
            {
                string query = "SELECT 1;";
                var cmd = new MySqlCommand(query, _dbConnection.Connection);
                var reader = await cmd.ExecuteScalarAsync();
                Assert.Same("hello", reader);
            }
        }
    }
}
