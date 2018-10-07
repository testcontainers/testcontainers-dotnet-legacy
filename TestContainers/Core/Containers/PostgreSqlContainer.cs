using System;
using System.Threading.Tasks;
using Npgsql;
using Polly;

namespace TestContainers.Core.Containers
{
    public sealed class PostgreSqlContainer : DatabaseContainer
    {
        public const string Image = "postgres";
        public const string DefaultTag = "9.6.8";
        public const int PostgreSqlPort = 5432;

        public PostgreSqlContainer(string tag) : base($"{Image}:{tag}") { }
        public PostgreSqlContainer() : this(DefaultTag) { }

        public override string GetConnectionString() => 
            $"Server={GetDockerHostIpAddress()};Port={GetMappedPort(PostgreSqlPort)};Database={DatabaseName};User Id={UserName};Password={Password}";

        protected override string GetTestQueryString() => "SELECT 1";

        protected override void Configure()
        {
            AddExposedPort(PostgreSqlPort);
            AddEnv("POSTGRES_DB", DatabaseName);
            AddEnv("POSTGRES_USER", UserName);
            AddEnv("POSTGRES_PASSWORD", Password);
            SetCommand("postgres");
        } 

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();

            var connection = new NpgsqlConnection(GetConnectionString());

            var result = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(2))
                .WrapAsync(Policy
                    .Handle<NpgsqlException>()
                    .WaitAndRetryForeverAsync(iteration => TimeSpan.FromSeconds(10)))
                .ExecuteAndCaptureAsync(async () =>
                {
                    // ReSharper disable AccessToDisposedClosure
                    await connection.OpenAsync();
                    var cmd = new NpgsqlCommand(GetTestQueryString(), connection);
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