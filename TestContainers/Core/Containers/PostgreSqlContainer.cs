using System;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using Polly;

namespace TestContainers.Core.Containers
{
    public sealed class PostgreSqlContainer : DatabaseContainer
    {
        public const string IMAGE = "postgres";
        public const string DEFAULT_TAG = "9.6.8";
        public const int POSTGRESQL_PORT = 5432;
        private readonly Type _npgsqlConnection;
        private readonly Type _npgsqlCommand;
        private readonly Type _npgsqlException;

        public override string DatabaseName => base.DatabaseName ?? _databaseName;

        public override string UserName => base.UserName ?? _userName;

        public override string Password => base.Password ?? _password;

        string _databaseName = "test";
        string _userName = "postgres";
        string _password = "Password123";

        public PostgreSqlContainer() : base()
        {
            var assembly = Assembly.Load("Npgsql");
            _npgsqlConnection = assembly.GetType("Npgsql.NpgsqlConnection", true);
            _npgsqlCommand = assembly.GetType("Npgsql.NpgsqlCommand", true);
            _npgsqlException = assembly.GetType("Npgsql.NpgsqlException", true);
        }

        public override string ConnectionString => $"Host={GetDockerHostIpAddress()};Username={UserName};pwd={Password}";

        protected override string TestQueryString => "SELECT 1";

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();

            var connection = (DbConnection) Activator.CreateInstance(_npgsqlConnection, ConnectionString);

            var result = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(2))
                .WrapAsync(Policy
                    .Handle<Exception>(e => _npgsqlException.IsInstanceOfType(e))
                    .WaitAndRetryForeverAsync(
                        iteration => TimeSpan.FromSeconds(10)))
                .ExecuteAndCaptureAsync(async () =>
                {
                    await connection.OpenAsync();

                    var cmd = (DbCommand) Activator.CreateInstance(_npgsqlCommand, TestQueryString, connection);
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