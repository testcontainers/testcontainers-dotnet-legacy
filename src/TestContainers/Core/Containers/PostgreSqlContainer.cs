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

        public const string TAG = "9.6.8";

        public const int PORT = 5432;

        private const string DEFAULT_DATABASE_NAME = "test";

        private const string DEFAULT_USERNAME = "postgres";

        private const string DEFAULT_PASSWORD = "Password123";

        public override string ConnectionString => $"Host={GetDockerHostIpAddress()};Username={UserName};pwd={Password}";

        public override string DatabaseName => base.DatabaseName ?? DEFAULT_DATABASE_NAME;

        public override string UserName => base.UserName ?? DEFAULT_USERNAME;

        public override string Password => base.Password ?? DEFAULT_PASSWORD;

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
