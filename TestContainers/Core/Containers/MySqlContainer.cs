using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Polly;

namespace TestContainers.Core.Containers
{
    public sealed class MySqlContainer : DatabaseContainer
    {
        public const string Image = "mysql";
        public const string DefaultTag = "8.0.12";
        public const int MySqlPort = 3306;

        public MySqlContainer(string tag) : base($"{Image}:{tag}") { }
        public MySqlContainer() : this(DefaultTag) { }
        
        public override string GetConnectionString() => $"Server={GetDockerHostIpAddress()};Port={GetMappedPort(MySqlPort)};Uid={UserName};pwd={Password};SslMode=none;";

        protected override string GetTestQueryString() => "SELECT 1";

        protected override void Configure()
        {
            AddExposedPort(MySqlPort);
            AddEnv("MYSQL_DATABASE", DatabaseName);
            AddEnv("MYSQL_USER", UserName);
            AddEnv("MYSQL_PASSWORD", Password);

            if (!EnvironmentVariables.ContainsKey("MYSQL_ROOT_PASSWORD"))
                AddEnv("MYSQL_ROOT_PASSWORD", "root-secret");
        }

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();

            var connection = new MySqlConnection(GetConnectionString());

            var result = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(2))
                .WrapAsync(Policy
                    .Handle<MySqlException>()
                    .WaitAndRetryForeverAsync(iteration => TimeSpan.FromSeconds(10)))
                .ExecuteAndCaptureAsync(async () =>
                {
                    // ReSharper disable AccessToDisposedClosure
                    await connection.OpenAsync();
                    var cmd = new MySqlCommand(GetTestQueryString(), connection);
                    // ReSharper restore AccessToDisposedClosure

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