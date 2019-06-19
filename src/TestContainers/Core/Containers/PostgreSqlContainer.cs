using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Npgsql;
using Polly;

namespace TestContainers.Core.Containers
{
    public sealed class PostgreSqlContainer : DatabaseContainer
    {
        public const string IMAGE = "postgres";
        public const string DEFAULT_TAG = "9.6.8";
        public const int POSTGRESQL_PORT = 5432;

        public override string DatabaseName => base.DatabaseName ?? _databaseName;

        public override string UserName => base.UserName ?? _userName;

        public override string Password => base.Password ?? _password;

        string _databaseName = "test";
        string _userName = "postgres";
        string _password = "Password123";

        public override string ConnectionString
        {
            get
            {
                if (Port.HasValue)
                {
                    return $"Host={GetDockerHostIpAddress()};Port={Port.Value};Username={UserName};pwd={Password}";
                }

                return $"Host={GetDockerHostIpAddress()};Username={UserName};pwd={Password}";
            }
        }

        protected override string TestQueryString => "SELECT 1";

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();

            var connection = new NpgsqlConnection(ConnectionString);

            var result = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(2))
                .WrapAsync(Policy
                    .Handle<NpgsqlException>()
                    .Or<SocketException>()
                    .WaitAndRetryForeverAsync(
                        iteration => TimeSpan.FromSeconds(10)))
                .ExecuteAndCaptureAsync(async () =>
                {
                    await connection.OpenAsync();

                    var cmd = new NpgsqlCommand(TestQueryString, connection);
                    await cmd.ExecuteScalarAsync();
                });

            if (result.Outcome == OutcomeType.Failure)
            {
                connection.Dispose();
                throw new Exception(result.FinalException.Message, result.FinalException);
            }
        }
    }
}
