using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestContainers.Core.Builders;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.ContainerTests
{
    public class RedisWithConfigFixture : IAsyncLifetime
    {
        public string ConnectionString => Container.ConnectionString;
        RedisContainer Container { get; }

        public RedisWithConfigFixture() =>
                Container = new GenericContainerBuilder<RedisContainer>()
                .Begin()
                .WithImage("redis:4.0.8")
                .WithExposedPorts(6379)
                .WithMountPoints((Directory.GetCurrentDirectory() + "/ContainerTests/Basic/master-6379.conf", "/usr/local/etc/redis/redis.conf", "bind"))
                .WithCmd("/usr/local/etc/redis/redis.conf")
                .Build();

        public Task InitializeAsync() => Container.Start();

        public Task DisposeAsync() => Container.Stop();
    }

    public class RedisWithConfigFixtureTests : IClassFixture<RedisWithConfigFixture>
    {
        readonly IDatabase _cache;
        public RedisWithConfigFixtureTests(RedisWithConfigFixture fixture) => _cache = ConnectionMultiplexer.Connect(fixture.ConnectionString).GetDatabase();

        [Fact]
        public async Task SimpleTest()
        {
            await _cache.StringSetAsync("myName", "Gurpreet");

            var myName = await _cache.StringGetAsync("myName");

            Assert.Equal("Gurpreet", myName);
        }
    }
}
