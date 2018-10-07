using Polly;
using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace TestContainers.Core.Containers
{
    public sealed class RedisContainer : DatabaseContainer
    {
        public const string Image = "redis";
        public const string DefaultTag = "4.0.8";
        public const int RedisPort = 6379;

        public RedisContainer(string tag) : base($"{Image}:{tag}") { }
        public RedisContainer() : this(DefaultTag) { }

        public override string GetConnectionString() => $"{GetDockerHostIpAddress()}:{GetMappedPort(RedisPort)}";

        protected override string GetTestQueryString() => "blank";

        protected override async Task WaitUntilContainerStarted()
        {
            await base.WaitUntilContainerStarted();

            var policyResult = await Policy
               .TimeoutAsync(TimeSpan.FromMinutes(2))
               .WrapAsync(Policy.Handle<RedisConnectionException>()
                   .WaitAndRetryForeverAsync(
                        iteration => TimeSpan.FromSeconds(10),
                       (exception, timespan) => Console.WriteLine(exception.Message)))
               .ExecuteAndCaptureAsync(() => ConnectionMultiplexer.ConnectAsync(GetConnectionString()));

            if (policyResult.Outcome == OutcomeType.Failure)
                throw new Exception(policyResult.FinalException.Message);
        }
    }
}
