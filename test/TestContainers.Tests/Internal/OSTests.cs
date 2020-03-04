using System.Collections.Generic;
using System.IO;
using TestContainers.Internal;
using Xunit;

namespace TestContainers.Tests.Internal
{
    public class OsTests
    {
        public class NormalizePathTests : OsTests
        {
            private static readonly char DirectorySeparator = Path.DirectorySeparatorChar;

            [Theory]
            [MemberData(nameof(ShouldConvertPathsToOsSpecificPathData))]
            public void ShouldConvertPathsToOsSpecificPath(string testPath, string expectedPath)
            {
                // act
                var result = OS.NormalizePath(testPath);

                // assert
                Assert.Equal(expectedPath, result);
            }

            public static readonly IEnumerable<object[]> ShouldConvertPathsToOsSpecificPathData = new List<object[]>
            {
                new object[] {"/linux/folder", $"{DirectorySeparator}linux{DirectorySeparator}folder"},
                new object[] {"linux/relative/path", $"linux{DirectorySeparator}relative{DirectorySeparator}path"},
                new object[] {"C:\\windows\\folder", $"C:{DirectorySeparator}windows{DirectorySeparator}folder"},
                new object[]
                {
                    "windows\\relative\\path", $"windows{DirectorySeparator}relative{DirectorySeparator}path"
                }
            };
        }
    }
}
