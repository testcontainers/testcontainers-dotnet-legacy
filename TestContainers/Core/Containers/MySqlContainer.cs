using System;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Polly;

namespace TestContainers.Core.Containers
{
    public class MySqlContainer : DatabaseContainer
    {
        public const string NAME = "mysql";
        public const string IMAGE = "mysql";
        public const int MYSQL_PORT = 3306;

        public override string DatabaseName => base.DatabaseName ?? _databaseName;

        public override string UserName => base.UserName ?? _userName;

        public override string Password => base.Password ?? _password;

        string _databaseName = "test";
        string _userName = "root";
        string _password = "Password123";

        public MySqlContainer() : base()
        {

        }

        int GetMappedPort(int portNo) => portNo;


        public override string ConnectionString => $"Server={GetDockerHostIpAddress()};UID={UserName};pwd={Password};SslMode=none;";

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