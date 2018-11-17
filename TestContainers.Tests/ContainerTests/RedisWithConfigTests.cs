using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using TestContainers.Core.Builders;
using TestContainers.Core.Containers;
using Xunit;

namespace TestContainers.Tests.ContainerTests
{
    public class RedisWithConfigFixture : IAsyncLifetime
    {
        public string ConnectionString => Container.GetConnectionString();
        RedisContainer Container { get; }

        bool RunningInCI { get; } = Environment.GetEnvironmentVariable("APPVEYOR") != null && EnvironmentHelper.IsWindows();

        string BaseDirectory => RunningInCI ? "X:/host/RedisConfigs" : AppContext.BaseDirectory;

        public RedisWithConfigFixture() =>
                Container = new GenericContainerBuilder<RedisContainer>()
                .WithImage("redis:4.0.8")
                .WithMountPoint($"{BaseDirectory}/master-6379.conf", "/usr/local/etc/redis/redis.conf", "bind")
                .WithCommand("/usr/local/etc/redis/redis.conf")
                .Build();

        public Task InitializeAsync() => Container.Start();

        public Task DisposeAsync() => Container.Stop();
    }

    public class RedisWithConfigFixtureTests : IClassFixture<RedisWithConfigFixture>
    {
        private readonly IDatabase _cache;

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
