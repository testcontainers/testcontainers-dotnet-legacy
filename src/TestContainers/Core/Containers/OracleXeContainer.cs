using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using Polly;

namespace TestContainers.Core.Containers
{
    public sealed class OracleXeContainer : DatabaseContainer
    {
        public const string IMAGE = "oracle/database";
        public const string DEFAULT_TAG = "11.2.0.2-xe";
        public const int ORACLE_PORT = 1521;

        public override string DatabaseName => base.DatabaseName ?? _databaseName;

        public override string UserName => base.UserName ?? _userName;

        public override string Password => base.Password ?? _password;

        string _databaseName = "test";
        string _userName = "system";
        string _password = "s3cr3t";

        public override string ConnectionString => $"Data Source=(description=(address=(protocol=tcp)(host={GetDockerHostIpAddress()})(port={GetMappedPort(ORACLE_PORT)}))(connect_data=(sid=XE)));User Id={UserName};Password={Password};";

        protected override string TestQueryString => "SELECT 1 FROM DUAL";

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();

            var connection = new OracleConnection(ConnectionString);

            var result = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(4))
                .WrapAsync(Policy
                    .Handle<OracleException>()
                    .Or<SocketException>()
                    .WaitAndRetryForeverAsync(
                        iteration => TimeSpan.FromSeconds(10)))
                .ExecuteAndCaptureAsync(async () =>
                {
                    await connection.OpenAsync();

                    var cmd = new OracleCommand(TestQueryString, connection);
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
