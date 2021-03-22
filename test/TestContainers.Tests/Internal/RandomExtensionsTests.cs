using System;
using TestContainers.Internal;
using Xunit;

namespace TestContainers.Tests.Internal
{
    public class RandomExtensionsTests
    {
        private readonly Random _random;

        protected RandomExtensionsTests()
        {
            _random = new Random();
        }

        public class NextAlphaNumeric : RandomExtensionsTests
        {
            [Fact]
            public void ShouldReturnCorrectNumberOfCharacters()
            {
                // arrange
                var length = _random.Next(0, int.MaxValue / 16);

                // act
                var result = _random.NextAlphaNumeric(length);

                // assert
                Assert.Equal(length, result.Length);
            }
        }
    }
}
