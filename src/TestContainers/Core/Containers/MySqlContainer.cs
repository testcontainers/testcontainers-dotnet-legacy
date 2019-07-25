using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Polly;

namespace TestContainers.Core.Containers
{
    public sealed class MySqlContainer : DatabaseContainer
    {
        public const string IMAGE = "mysql";

        public const string TAG = "5.7";

        public const int PORT = 3306;

        private const string DEFAULT_DATABASE_NAME = "test";

        private const string DEFAULT_USERNAME = "root";

        private const string DEFAULT_PASSWORD = "Password123";

        public override string ConnectionString => $"Server={GetDockerHostIpAddress()};UID={UserName};pwd={Password};SslMode=none;";

        public override string DatabaseName => base.DatabaseName ?? DEFAULT_DATABASE_NAME;

        public override string UserName => base.UserName ?? DEFAULT_USERNAME;

        public override string Password => base.Password ?? DEFAULT_PASSWORD;

        protected override string TestQueryString => "SELECT 1";

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();

            var connection = new MySqlConnection(ConnectionString);

            var result = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(2))
                .WrapAsync(Policy
                    .Handle<MySqlException>()
                    .WaitAndRetryForeverAsync(
                        iteration => TimeSpan.FromSeconds(10)))
                .ExecuteAndCaptureAsync(async () =>
                {
                    await connection.OpenAsync();

                    var cmd = new MySqlCommand(TestQueryString, connection);
                    await cmd.ExecuteScalarAsync();
                });

            if (result.Outcome == OutcomeType.Failure)
            {
                connection.Dispose();
                throw new Exception(result.FinalException.Message);
            }
        }
    }
}