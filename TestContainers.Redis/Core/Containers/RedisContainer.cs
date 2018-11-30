using Polly;
using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Linq;

namespace TestContainers.Core.Containers
{
    public sealed class RedisContainer : DatabaseContainer
    {
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
                   .Handle<RedisConnectionException>()
                   .WaitAndRetryForeverAsync(
                       iteration => TimeSpan.FromSeconds(10),
                       (exception, timespan) => Console.WriteLine(exception.Message)))
               .ExecuteAndCaptureAsync(() =>
                   ConnectionMultiplexer.ConnectAsync(ConnectionString));

            if (policyResult.Outcome == OutcomeType.Failure)
                throw new Exception(policyResult.FinalException.Message);
        }
    }
}
