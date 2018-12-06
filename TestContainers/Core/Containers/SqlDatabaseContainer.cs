using System;
using System.Data.Common;
using System.Threading.Tasks;
using Polly;

namespace TestContainers.Core.Containers
{
    public abstract class SqlDatabaseContainer : DatabaseContainer
    {
        protected abstract Type ConnectionType { get; }
        protected abstract Type ExceptionType { get; }
        protected abstract Type CommandType { get; }

        public string TestQueryString { get; set; } = "SELECT 1";

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();

            var connection = CreateConnection();

            var result = await Policy
                .TimeoutAsync(TimeSpan.FromSeconds(GetStartupTimeoutSeconds))
                .WrapAsync(Policy
                    .Handle<Exception>(e => ExceptionType.IsInstanceOfType(e))
                    .WaitAndRetryForeverAsync(
                        iteration => TimeSpan.FromSeconds(GetConnectTimeoutSeconds)))
                .ExecuteAndCaptureAsync(async () =>
                {
                    await connection.OpenAsync();

                    using (var cmd = CreateTestCommand(connection))
                    {
                        await cmd.ExecuteScalarAsync();
                    }
                });

            if (result.Outcome == OutcomeType.Failure)
            {
                connection.Dispose();
                throw new Exception(result.FinalException.Message);
            }
        }

        protected virtual DbConnection CreateConnection()
        {
            return (DbConnection) Activator.CreateInstance(ConnectionType, ConnectionString);
        }

        protected virtual DbCommand CreateTestCommand(DbConnection connection)
        {
            return (DbCommand) Activator.CreateInstance(CommandType, TestQueryString, connection);
        }
    }
}