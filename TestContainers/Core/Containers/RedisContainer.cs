using Polly;
using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace TestContainers.Core.Containers
{
    public class RedisContainer : DatabaseContainer
    {
        public override string ConnectionString => GetDockerHostIpAddress();
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
