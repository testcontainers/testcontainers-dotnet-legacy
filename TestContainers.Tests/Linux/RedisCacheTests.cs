using System;
using Xunit;
using StackExchange.Redis;
using System.Threading.Tasks;
using TestContainers;
using Polly;
using Newtonsoft.Json;
using TestContainers.Core.Containers;
using System.Linq;

namespace TestContainers.Tests.Linux
{
    public class RedisCacheFixture : IAsyncLifetime
    {
        public IDatabase Cache { get; private set; }
        Container _container { get; }

        public RedisCacheFixture() =>
             _container = new ContainerBuilder()
                .Begin()
                .WithImage("redis:4.0.8")
                .WithExposedPorts(6379)
                .Build();

        public async Task InitializeAsync()
        {
            await _container.Start();

            var policyResult = await Policy
                .TimeoutAsync(TimeSpan.FromMinutes(2))
                .WrapAsync(Policy
                    .Handle<RedisConnectionException>()
                    .WaitAndRetryForeverAsync(iteration => TimeSpan.FromSeconds(10)))
                .ExecuteAndCaptureAsync(() =>
                {
                    var ipAddress = _container.ContainerInspectResponse.NetworkSettings.Networks.First().Value.IPAddress;
                    return ConnectionMultiplexer.ConnectAsync(ipAddress);
                });

            if (policyResult.Outcome == OutcomeType.Failure)
                throw new Exception(policyResult.FinalException.Message);

            Cache = policyResult.Result.GetDatabase();
        }

        public Task DisposeAsync() => _container.Stop();
    }

    public class RedisCacheTests : IClassFixture<RedisCacheFixture>
    {
        readonly IDatabase _cache;
        public RedisCacheTests(RedisCacheFixture fixture) => _cache = fixture.Cache;

        [Fact]
        public async Task SimpleTest()
        {
            await _cache.StringSetAsync("myName", "Gurpreet");

            var myName = await _cache.StringGetAsync("myName");

            Assert.Equal("Gurpreet", myName);
        }
    }
}
