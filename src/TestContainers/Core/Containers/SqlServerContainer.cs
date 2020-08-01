using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Polly;

namespace TestContainers.Core.Containers
{
    public sealed class SqlServerContainer : DatabaseContainer
    {
        public const string NAME = "mssql";

        public const string IMAGE_LNX = "mcr.microsoft.com/mssql/server" ;
        public const string IMAGE_WIN = "microsoft/mssql-server-windows-developer";

        public const string DEFAULT_TAG = "2017-latest";
        public const int SQLSERVER_PORT = 1433;

        public override string DatabaseName => base.DatabaseName ?? _databaseName;

        public override string UserName => base.UserName ?? _userName;

        public override string Password => base.Password ?? _password;

        readonly string _databaseName = "test";
        readonly string _userName = "SA";
        readonly string _password = "Password123";

        public override string ConnectionString => $"Server={GetDockerHostIpAddress()},{GetMappedPort(SQLSERVER_PORT)};User Id={UserName}; Password={Password};";

        protected override string TestQueryString => "SELECT 1";

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();

            var connection = new SqlConnection(ConnectionString);

            var result = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(2))
                .WrapAsync(Policy
                    .Handle<SqlException>()
                    .WaitAndRetryForeverAsync(
                        iteration => TimeSpan.FromSeconds(10)))
                .ExecuteAndCaptureAsync(async () =>
                {
                    await connection.OpenAsync();

                    var cmd = new SqlCommand(TestQueryString, connection);
                    var reader = (await cmd.ExecuteScalarAsync());
                });

            if (result.Outcome == OutcomeType.Failure)
            {
                connection.Dispose();
                throw new Exception(result.FinalException.Message);
            }
        }
    }
}
