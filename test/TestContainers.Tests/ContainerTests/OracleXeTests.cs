using Xunit;
using System.Threading.Tasks;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using TestContainers.Core.Containers;
using TestContainers.Core.Builders;

namespace TestContainers.Tests.ContainerTests
{
    public class OracleXeFixture : IAsyncLifetime
    {
        public string ConnectionString => Container.ConnectionString;
        OracleXeContainer Container { get; }

        public OracleXeFixture() =>
             Container = new DatabaseContainerBuilder<OracleXeContainer>()
                .Begin()
                .WithImage($"{OracleXeContainer.IMAGE}:{OracleXeContainer.DEFAULT_TAG}")
                .WithExposedPorts(OracleXeContainer.ORACLE_PORT)
                .WithEnv(("ORACLE_PWD", "s3cr3t"))
                .WithShmSize(1024 * 1024 * 1024)
                .Build();

        public Task InitializeAsync() => Container.Start();

        public Task DisposeAsync() => Container.Stop();
    }

    public class OracleXeTests : IClassFixture<OracleXeFixture>
    {
        readonly OracleConnection _connection;
        public OracleXeTests(OracleXeFixture fixture) => _connection = new OracleConnection(fixture.ConnectionString);

        [Fact]
        public async Task SimpleTest()
        {
            const string query = "SELECT 1 FROM DUAL";
            await _connection.OpenAsync();
            var cmd = new OracleCommand(query, _connection);
            var reader = (await cmd.ExecuteScalarAsync());
            Assert.Equal(1.0m, reader);

            _connection.Close();
        }
    }
}
