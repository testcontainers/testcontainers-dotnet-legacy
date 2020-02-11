using TestContainers.Containers.WaitStrategies;
using Xunit;

namespace TestContainers.Tests.Containers.WaitStrategies
{
    public class NoWaitStrategyTest
    {
        private readonly IWaitStrategy _strategy;

        public NoWaitStrategyTest()
        {
            _strategy = new NoWaitStrategy();
        }

        [Fact]
        public void ShouldReturnSuccessImmediately()
        {
            // act
            var result = _strategy.WaitUntil(null, null);

            // assert
            Assert.True(result.IsCompletedSuccessfully);
        }
    }
}
