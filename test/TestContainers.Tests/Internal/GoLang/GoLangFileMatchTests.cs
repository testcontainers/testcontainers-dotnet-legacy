using System;
using System.Collections.Generic;
using TestContainers.Internal;
using TestContainers.Internal.GoLang;
using Xunit;

namespace TestContainers.Tests.Internal.GoLang
{
    /// <summary>
    /// Copied directly from
    /// https://github.com/docker-java/docker-java/blob/master/docker-java/src/test/java/com/github/dockerjava/core/GoLangFileMatchTest.java
    /// </summary>
    public class GoLangFileMatchTests
    {
        public static IEnumerable<object[]> TestData()
        {
            return new List<object[]>
            {
                new object[] {new MatchTestCase("", "abc", false, false)},
                new object[] {new MatchTestCase("abc", "abc", true, false)},
                new object[] {new MatchTestCase("*", "abc", true, false)},
                new object[] {new MatchTestCase("*c", "abc", true, false)},
                new object[] {new MatchTestCase("a*", "a", true, false)},
                new object[] {new MatchTestCase("a*", "abc", true, false)},
                new object[] {new MatchTestCase("a*", "ab/c", true, false)},
                new object[] {new MatchTestCase("a*/b", "abc/b", true, false)},
                new object[] {new MatchTestCase("a*/b", "a/c/b", false, false)},
                new object[] {new MatchTestCase("a*b*c*d*e*/f", "axbxcxdxe/f", true, false)},
                new object[] {new MatchTestCase("a*b*c*d*e*/f", "axbxcxdxexxx/f", true, false)},
                new object[] {new MatchTestCase("a*b*c*d*e*/f", "axbxcxdxe/xxx/f", false, false)},
                new object[] {new MatchTestCase("a*b*c*d*e*/f", "axbxcxdxexxx/fff", false, false)},
                new object[] {new MatchTestCase("a*b?c*x", "abxbbxdbxebxczzx", true, false)},
                new object[] {new MatchTestCase("a*b?c*x", "abxbbxdbxebxczzy", false, false)},
                new object[] {new MatchTestCase("ab[c]", "abc", true, false)},
                new object[] {new MatchTestCase("ab[b-d]", "abc", true, false)},
                new object[] {new MatchTestCase("ab[e-g]", "abc", false, false)},
                new object[] {new MatchTestCase("ab[^c]", "abc", false, false)},
                new object[] {new MatchTestCase("ab[^b-d]", "abc", false, false)},
                new object[] {new MatchTestCase("ab[^e-g]", "abc", true, false)},
                new object[] {new MatchTestCase("a\\*b", "a*b", true, false)},
                new object[] {new MatchTestCase("a\\*b", "ab", false, false)},
                new object[] {new MatchTestCase("a?b", "a☺b", true, false)},
                new object[] {new MatchTestCase("a[^a]b", "a☺b", true, false)},
                new object[] {new MatchTestCase("a???b", "a☺b", false, false)},
                new object[] {new MatchTestCase("a[^a][^a][^a]b", "a☺b", false, false)},
                new object[] {new MatchTestCase("[a-ζ]*", "α", true, false)},
                new object[] {new MatchTestCase("*[a-ζ]", "A", false, false)},
                new object[] {new MatchTestCase("a?b", "a/b", false, false)},
                new object[] {new MatchTestCase("a*b", "a/b", false, false)},
                new object[] {new MatchTestCase("[\\]a]", "]", true, false)},
                new object[] {new MatchTestCase("[\\-]", "-", true, false)},
                new object[] {new MatchTestCase("[x\\-]", "x", true, false)},
                new object[] {new MatchTestCase("[x\\-]", "-", true, false)},
                new object[] {new MatchTestCase("[x\\-]", "z", false, false)},
                new object[] {new MatchTestCase("[\\-x]", "x", true, false)},
                new object[] {new MatchTestCase("[\\-x]", "-", true, false)},
                new object[] {new MatchTestCase("[\\-x]", "a", false, false)},
                new object[] {new MatchTestCase("[]a]", "]", false, true)},
                new object[] {new MatchTestCase("[-]", "-", false, true)},
                new object[] {new MatchTestCase("[x-]", "x", false, true)},
                new object[] {new MatchTestCase("[x-]", "-", false, true)},
                new object[] {new MatchTestCase("[x-]", "z", false, true)},
                new object[] {new MatchTestCase("[-x]", "x", false, true)},
                new object[] {new MatchTestCase("[-x]", "-", false, true)},
                new object[] {new MatchTestCase("[-x]", "a", false, true)},
                new object[] {new MatchTestCase("\\", "a", false, true)},
                new object[] {new MatchTestCase("[a-b-c]", "a", false, true)},
                new object[] {new MatchTestCase("[", "a", false, true)},
                new object[] {new MatchTestCase("[^", "a", false, true)},
                new object[] {new MatchTestCase("[^bc", "a", false, true)},
                new object[] {new MatchTestCase("a[", "a", false, true)},
                new object[] {new MatchTestCase("a[", "ab", false, true)},
                new object[] {new MatchTestCase("*x", "xxx", true, false)},
                new object[] {new MatchTestCase("a", "a/b/c", true, false)},
                new object[] {new MatchTestCase("*/b", "a/b/c", true, false)},
                new object[] {new MatchTestCase("**/b/*/d", "a/b/c/d", true, false)},
                new object[] {new MatchTestCase("**/c", "a/b/c", true, false)}
            };
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void TestMatch(MatchTestCase testCase)
        {
            var pattern = testCase.Pattern;
            var s = testCase.Input;

            if (GoLangFileMatch.IsWindows)
            {
                if (pattern.IndexOf('\\') >= 0)
                {
                    // no escape allowed on windows.
                    return;
                }

                pattern = OS.NormalizePath(pattern);
                s = OS.NormalizePath(s);
            }

            try
            {
                var matched = GoLangFileMatch.Match(pattern, s);
                if (testCase.ShouldThrowException)
                {
                    Assert.False(testCase.ShouldThrowException, "Exception was expected to be thrown but not thrown");
                }

                Assert.Equal(testCase.ShouldMatch, matched);
            }
            catch (ArgumentException)
            {
                if (!testCase.ShouldThrowException)
                {
                    throw;
                }
            }
        }

        public sealed class MatchTestCase
        {
            public string Pattern { get; }

            public string Input { get; }

            public bool ShouldMatch { get; }

            public bool ShouldThrowException { get; }

            public MatchTestCase(string pattern, string input, bool shouldMatch, bool shouldThrowException)
            {
                Pattern = pattern;
                Input = input;
                ShouldMatch = shouldMatch;
                ShouldThrowException = shouldThrowException;
            }
        }
    }
}
