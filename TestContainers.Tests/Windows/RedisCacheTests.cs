using System;
using Xunit;
using StackExchange.Redis;
using System.Threading.Tasks;
using TestContainers;
using Polly;
using TestContainers.Core.Containers;
using System.Linq;
using TestContainers.Core.Builders;

namespace TestContainers.Tests.Windows
{
    public class RedisCacheFixture : IAsyncLifetime
    {
        public IDatabase Cache { get; private set; }
        Container _container { get; }

        public RedisCacheFixture() => _container = new GenericContainerBuilder()
               .Begin()
               .WithImage("alexellis2/redis-windows:latest")
               .WithExposedPorts(6379)
               .Build();

        public async Task InitializeAsync()
        {
            await _container.Start();

            Cache = (await ConnectionMultiplexer.ConnectAsync(_container.GetContainerIpAddress())).GetDatabase();
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
