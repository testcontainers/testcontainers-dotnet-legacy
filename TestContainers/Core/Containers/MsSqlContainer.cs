using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Polly;

namespace TestContainers.Core.Containers
{
    public sealed class MsSqlContainer : DatabaseContainer
    {
        public const string NAME = "mssqlserver";
        public const string IMAGE = "mcr.microsoft.com/mssql/server";
        public const string DEFAULT_TAG = "2017-latest-ubuntu";
        
        /// <summary>This will run the container using the Developer Edition (this is the default if no MSSQL_PID environment variable is supplied)</summary>
        public const string EDITION_DEVELOPER = "Developer";

        /// <summary>This will run the container using the Express Edition</summary>
        public const string EDITION_EXPRESS = "Express";

        /// <summary>This will run the container using the Standard Edition</summary>
        public const string EDITION_STANDARD = "Standard";

        /// <summary>This will run the container using the Enterprise Edition</summary>
        public const string EDITION_ENTERPRISE = "Enterprise";

        /// <summary>This will run the container using the Enterprise Edition Core</summary>
        public const string EDITION_ENTERPRISECORE = "EnterpriseCore";

        public const int MSSQL_PORT = 1433;

        protected override int GetStartupTimeoutSeconds => 240;
        protected override int GetConnectTimeoutSeconds => 240;

        public override string DatabaseName => base.DatabaseName ?? _databaseName;

        public override string UserName => base.UserName ?? _userName;

        public override string Password
        {
            get => base.Password ?? _password;
            set
            {
                base.Password = value;
                EnvironmentVariables = PrepareEnvironmentVariables().ToArray();
            }
        }

        public string ProductEdition { get; set; }

        string _databaseName = "test";
        string _userName = "SA";
        string _password = "A_Str0ng_Required_Password";

        public MsSqlContainer() : base()
        {
            LicenseAcceptance.AssertLicenseAccepted(IMAGE);
            EnvironmentVariables = PrepareEnvironmentVariables().ToArray();
        }

        public List<(string key, string value)> PrepareEnvironmentVariables()
        {
            var environmentVariables = new List<(string key, string value)>(3)
            {
                ("ACCEPT_EULA", "Y"),
                ("SA_PASSWORD", Password)
            };
            if (!string.IsNullOrWhiteSpace(ProductEdition))
            {
                environmentVariables.Add(("MSSQL_PID", ProductEdition));
            }

            return environmentVariables;
        }

        int GetMappedPort(int portNo) => portNo;


        public override string ConnectionString => new SqlConnectionStringBuilder()
        {
            DataSource = GetDockerHostIpAddress(),
            UserID = UserName,
            Password = Password,
            Encrypt = false
        }.ToString();

        protected override string TestQueryString => "SELECT 1";

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();

            var connection = new SqlConnection(ConnectionString);

            var result = await Policy
                .TimeoutAsync(TimeSpan.FromSeconds(GetConnectTimeoutSeconds))
                .WrapAsync(Policy
                    .Handle<SqlException>()
                    .WaitAndRetryForeverAsync(
                        iteration => TimeSpan.FromSeconds(10)))
                .ExecuteAndCaptureAsync(async () =>
                {
                    await connection.OpenAsync();

                    var cmd = new SqlCommand(TestQueryString, connection);
                    var reader = await cmd.ExecuteScalarAsync();
                });

            if (result.Outcome == OutcomeType.Failure)
            {
                connection.Dispose();
                throw new Exception(result.FinalException.Message);
            }
        }
    }
}