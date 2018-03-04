using System;
using Xunit;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace TestContainers.Tests
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
            var connection = await ConnectionMultiplexer.ConnectAsync("localhost");
            Cache = connection.GetDatabase();
        }

        public Task DisposeAsync() => _container.Stop();
    }


    public class RedisCacheTests : IClassFixture<RedisCacheFixture>
    {
        readonly IDatabase _cache;
        public RedisCacheTests(RedisCacheFixture fixture) => _cache = fixture.Cache;
        
        [Fact, Trait("Category", "LCOL")]
        public async Task SimpleTest()
        {
            await _cache.StringSetAsync("myName", "Gurpreet");

            var myName = await _cache.StringGetAsync("myName");

            Assert.Equal("Gurpreet", myName);
        }
    }
}
