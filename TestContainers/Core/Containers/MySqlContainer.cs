using System;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using Polly;

namespace TestContainers.Core.Containers
{
    public sealed class MySqlContainer : DatabaseContainer
    {
        public const string NAME = "mysql";
        public const string IMAGE = "mysql";
        public const int MYSQL_PORT = 3306;
        private readonly Type _mySqlConnection;
        private readonly Type _mySqlCommand;
        private readonly Type _mySqlException;

        public override string DatabaseName => base.DatabaseName ?? _databaseName;

        public override string UserName => base.UserName ?? _userName;

        public override string Password => base.Password ?? _password;

        string _databaseName = "test";
        string _userName = "root";
        string _password = "Password123";

        public MySqlContainer() : base()
        {
            var assembly = Assembly.Load("MySql.Data");
            _mySqlConnection = assembly.GetType("MySql.Data.MySqlClient.MySqlConnection", true);
            _mySqlCommand = assembly.GetType("MySql.Data.MySqlClient.MySqlCommand", true);
            _mySqlException = assembly.GetType("MySql.Data.MySqlClient.MySqlException", true);
        }

        int GetMappedPort(int portNo) => portNo;


        public override string ConnectionString => $"Server={GetDockerHostIpAddress()};UID={UserName};pwd={Password};SslMode=none;";

        protected override string TestQueryString => "SELECT 1";

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();

            var connection = (DbConnection) Activator.CreateInstance(_mySqlConnection, ConnectionString);

            var result = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(2))
                .WrapAsync(Policy
                    .Handle<Exception>(e => _mySqlException.IsInstanceOfType(e))
                    .WaitAndRetryForeverAsync(
                        iteration => TimeSpan.FromSeconds(10)))
                .ExecuteAndCaptureAsync(async () =>
                {
                    await connection.OpenAsync();

                    var cmd = (DbCommand) Activator.CreateInstance(_mySqlCommand, TestQueryString, connection);
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