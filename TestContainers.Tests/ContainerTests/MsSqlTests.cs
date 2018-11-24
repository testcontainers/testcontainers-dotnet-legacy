using System.Data.SqlClient;
using Xunit;
using System.Threading.Tasks;
using TestContainers.Core.Containers;
using TestContainers.Core.Builders;

namespace TestContainers.Tests.ContainerTests
{
    public class MsSqlFixture : IAsyncLifetime
    {
        public string ConnectionString => Container.ConnectionString;
        MsSqlContainer Container { get; }

        public MsSqlFixture() =>
             Container = new DatabaseContainerBuilder<MsSqlContainer>()
                .Begin()
                .WithImage($"{MsSqlContainer.IMAGE}:{MsSqlContainer.DEFAULT_TAG}")
                .WithExposedPorts(MsSqlContainer.MSSQL_PORT)
                .Build();

        public Task InitializeAsync() => Container.Start();

        public Task DisposeAsync() => Container.Stop();
    }

    public class MsSqlTests : IClassFixture<MsSqlFixture>
    {
        readonly SqlConnection _connection;
        public MsSqlTests(MsSqlFixture fixture) => _connection = new SqlConnection(fixture.ConnectionString);

        [Fact]
        public async Task SimpleTest()
        {
            string query = "SELECT 1;";
            await _connection.OpenAsync();
            var cmd = new SqlCommand(query, _connection);
            var reader = (await cmd.ExecuteScalarAsync());
            Assert.Equal(1, reader);

            _connection.Close();
        }
    }
}
