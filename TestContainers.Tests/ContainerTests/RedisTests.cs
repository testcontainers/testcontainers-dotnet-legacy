using Xunit;
using StackExchange.Redis;
using System.Threading.Tasks;
using TestContainers.Core.Containers;
using TestContainers.Core.Builders;

namespace TestContainers.Tests.ContainerTests
{
    public class RedisCacheFixture : IAsyncLifetime
    {
        public string ConnectionString => Container.GetConnectionString();
        RedisContainer Container { get; }

        public RedisCacheFixture() =>
                Container = new GenericContainerBuilder<RedisContainer>()
                .WithImage("redis:4.0.8")
                .Build();

        public Task InitializeAsync() => Container.StartAsync();

        public Task DisposeAsync() => Container.StopAsync();
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
