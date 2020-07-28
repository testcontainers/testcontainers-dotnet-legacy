using System.Threading.Tasks;
using StackExchange.Redis;
using TestContainers.Core.Builders;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.ContainerTests
{
    public class RedisCacheFixture : IAsyncLifetime
    {
        public string ConnectionString => Container.ConnectionString;
        RedisContainer Container { get; }

        public RedisCacheFixture() =>
                Container = new GenericContainerBuilder<RedisContainer>()
                .Begin()
                .WithImage("redis:4.0.8")
                .WithExposedPorts(RedisContainer.Port)
                .Build();

        public Task InitializeAsync() => Container.Start();

        public Task DisposeAsync() => Container.Stop();
    }


    public class RedisTests : IClassFixture<RedisCacheFixture>
    {
        readonly IDatabase _cache;
        public RedisTests(RedisCacheFixture fixture) => _cache = ConnectionMultiplexer.Connect(fixture.ConnectionString).GetDatabase();

        [Fact]
        public async Task SimpleTest()
        {
            await _cache.StringSetAsync("myName", "Gurpreet");

            var myName = await _cache.StringGetAsync("myName");

            Assert.Equal("Gurpreet", myName);
        }
    }
}


