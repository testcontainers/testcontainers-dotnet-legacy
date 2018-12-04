using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using Polly;

namespace TestContainers.Core.Containers
{
    public sealed class RedisContainer : DatabaseContainer
    {
        private readonly Func<string, TextWriter, Task> _connectAsync;
        private readonly Type _redisConnectionException;

        public RedisContainer() : base()
        {
            var assembly = Assembly.Load("StackExchange.Redis");
            var connectionMultiplexer = assembly.GetType("StackExchange.Redis.ConnectionMultiplexer", true);
            var connectAsync = connectionMultiplexer
                .GetMethod("ConnectAsync", new[] {typeof(string), typeof(TextWriter)});
            if (connectAsync == null)
            {
                throw new InvalidOperationException(
                    "ConnectionMultiplexer is lacking procedure Task<ConnectionMultiplexer> ConnectAsync(string, TextWriter)");
            }

            _connectAsync = (Func<string, TextWriter, Task>) connectAsync
                .CreateDelegate(typeof(Func<string, TextWriter, Task>));

            _redisConnectionException = assembly.GetType("StackExchange.Redis.RedisConnectionException", true);
        }

        public override string ConnectionString
        {
            get
            {
                var portBindings = PortBindings?.SingleOrDefault(p => p.ExposedPort == ExposedPorts.First());

                var port = portBindings?.PortBinding ?? ExposedPorts.First();

                return $"{GetDockerHostIpAddress()}:{port}";
            }
        }

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();

            var policyResult = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(2))
                .WrapAsync(Policy
                    .Handle<Exception>(e => _redisConnectionException.IsInstanceOfType(e))
                    .WaitAndRetryForeverAsync(
                        iteration => TimeSpan.FromSeconds(10),
                        (exception, timespan) => Console.WriteLine(exception.Message)))
                .ExecuteAndCaptureAsync(() => _connectAsync(ConnectionString, null));

            if (policyResult.Outcome == OutcomeType.Failure)
                throw new Exception(policyResult.FinalException.Message);
        }
    }
}