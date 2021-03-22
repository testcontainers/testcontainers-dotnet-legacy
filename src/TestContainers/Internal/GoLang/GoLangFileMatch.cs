using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TestContainers.Internal.GoLang
{
    /// <summary>
    /// GoLang FileMatch method
    /// Copied directly from
    /// https://github.com/docker-java/docker-java/blob/master/docker-java-core/src/main/java/com/github/dockerjava/core/GoLangFileMatch.java
    /// </summary>
    public static class GoLangFileMatch
    {
        internal static readonly bool IsWindows = Path.DirectorySeparatorChar == '\\';
        private const string PatternCharsToEscape = "\\.[]{}()*+-?^$|";

        // this needs to be thread safe because this will be used in a `AsParallel()` query
        // in DockerFileImage.cs
        private static readonly ConcurrentDictionary<string, Regex> RegexCache =
            new ConcurrentDictionary<string, Regex>();

        /// <summary>
        /// Returns the matching patterns for the given string
        /// </summary>
        /// <param name="patterns">patterns to test for</param>
        /// <param name="input">input</param>
        /// <returns>list of matched patterns</returns>
        public static List<string> Match(IEnumerable<string> patterns, string input)
        {
            return patterns.Where(pattern => Match(pattern, input)).ToList();
        }

        /// <summary>
        /// Returns if pattern matches the given string
        /// </summary>
        /// <param name="pattern">patterns to test for</param>
        /// <param name="input">input</param>
        /// <returns>whether the pattern matches the input</returns>
        public static bool Match(string pattern, string input)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return RegexCache
                .GetOrAdd(pattern, k => new Regex(BuildPattern(k), RegexOptions.Compiled))
                .IsMatch(input);
        }

        private static string BuildPattern(string pattern)
        {
            var patternStringBuilder = new StringBuilder("^");
            while (!string.IsNullOrWhiteSpace(pattern))
            {
                pattern = AppendChunkPattern(ref patternStringBuilder, pattern);

                if (!string.IsNullOrWhiteSpace(pattern))
                {
                    patternStringBuilder.Append(Quote(Path.DirectorySeparatorChar));
                }
            }

            patternStringBuilder.Append("(").Append(Quote(Path.DirectorySeparatorChar)).Append(".*").Append(")?$");
            return patternStringBuilder.ToString();
        }

        private static string Quote(char separatorChar)
        {
            if (PatternCharsToEscape.Contains(separatorChar))
            {
                return "\\" + separatorChar;
            }

            return separatorChar.ToString();
        }

        private static string AppendChunkPattern(ref StringBuilder patternStringBuilder, string pattern)
        {
            if (pattern.Equals("**") || pattern.StartsWith("**" + Path.DirectorySeparatorChar))
            {
                patternStringBuilder.Append("(")
                    .Append("[^").Append(Quote(Path.DirectorySeparatorChar)).Append("]*")
                    .Append("(")
                    .Append(Quote(Path.DirectorySeparatorChar)).Append("[^").Append(Quote(Path.DirectorySeparatorChar))
                    .Append("]*")
                    .Append(")*").Append(")?");

                return pattern.Substring(pattern.Length == 2 ? 2 : 3);
            }

            var inRange = false;
            var rangeFrom = 0;
            var rangeParseState = RangeParseState.CharExpected;
            var isEsc = false;
            for (var i = 0; i < pattern.Length; i++)
            {
                var c = pattern[i];
                switch (c)
                {
                    case '/':
                        if (!inRange)
                        {
                            if (!IsWindows && !isEsc)
                            {
                                // end of chunk
                                return pattern.Substring(i + 1);
                            }
                            else
                            {
                                patternStringBuilder.Append(Quote(c));
                            }
                        }
                        else
                        {
                            rangeParseState = NextStateAfterChar(rangeParseState);
                        }

                        isEsc = false;
                        break;
                    case '\\':
                        if (!inRange)
                        {
                            if (!IsWindows)
                            {
                                if (isEsc)
                                {
                                    patternStringBuilder.Append(Quote(c));
                                    isEsc = false;
                                }
                                else
                                {
                                    isEsc = true;
                                }
                            }
                            else
                            {
                                // end of chunk
                                return pattern.Substring(i + 1);
                            }
                        }
                        else
                        {
                            if (IsWindows || isEsc)
                            {
                                rangeParseState = NextStateAfterChar(rangeParseState);
                                isEsc = false;
                            }
                            else
                            {
                                isEsc = true;
                            }
                        }

                        break;
                    case '[':
                        if (!isEsc)
                        {
                            if (inRange)
                            {
                                throw new ArgumentException("[ not expected, closing bracket ] not yet reached");
                            }

                            rangeFrom = i;
                            rangeParseState = RangeParseState.CharExpected;
                            inRange = true;
                        }
                        else
                        {
                            if (!inRange)
                            {
                                patternStringBuilder.Append(c);
                            }
                            else
                            {
                                rangeParseState = NextStateAfterChar(rangeParseState);
                            }
                        }

                        isEsc = false;
                        break;
                    case ']':
                        if (!isEsc)
                        {
                            if (!inRange)
                            {
                                throw new ArgumentException("] is not expected, [ was not met");
                            }

                            if (rangeParseState == RangeParseState.CharExpectedAfterDash)
                            {
                                throw new ArgumentException("Character range not finished");
                            }

                            patternStringBuilder.Append(pattern.SubstringByIndexes(rangeFrom, i + 1));
                            inRange = false;
                        }
                        else
                        {
                            if (!inRange)
                            {
                                patternStringBuilder.Append(c);
                            }
                            else
                            {
                                rangeParseState = NextStateAfterChar(rangeParseState);
                            }
                        }

                        isEsc = false;
                        break;
                    case '*':
                        if (!inRange)
                        {
                            if (!isEsc)
                            {
                                patternStringBuilder.Append("[^").Append(Quote(Path.DirectorySeparatorChar))
                                    .Append("]*");
                            }
                            else
                            {
                                patternStringBuilder.Append(Quote(c));
                            }
                        }
                        else
                        {
                            rangeParseState = NextStateAfterChar(rangeParseState);
                        }

                        isEsc = false;
                        break;
                    case '?':
                        if (!inRange)
                        {
                            if (!isEsc)
                            {
                                patternStringBuilder.Append("[^").Append(Quote(Path.DirectorySeparatorChar))
                                    .Append("]");
                            }
                            else
                            {
                                patternStringBuilder.Append(Quote(c));
                            }
                        }
                        else
                        {
                            rangeParseState = NextStateAfterChar(rangeParseState);
                        }

                        isEsc = false;
                        break;
                    case '-':
                        if (!inRange)
                        {
                            patternStringBuilder.Append(Quote(c));
                        }
                        else
                        {
                            if (!isEsc)
                            {
                                if (rangeParseState != RangeParseState.CharOrDashExpected)
                                {
                                    throw new ArgumentException("- character not expected");
                                }

                                rangeParseState = RangeParseState.CharExpectedAfterDash;
                            }
                            else
                            {
                                rangeParseState = NextStateAfterChar(rangeParseState);
                            }
                        }

                        isEsc = false;
                        break;
                    default:
                        if (!inRange)
                        {
                            patternStringBuilder.Append(Quote(c));
                        }
                        else
                        {
                            rangeParseState = NextStateAfterChar(rangeParseState);
                        }

                        isEsc = false;
                        break;
                }
            }

            if (isEsc)
            {
                throw new ArgumentException("Escaped character missing");
            }

            if (inRange)
            {
                throw new ArgumentException("Character range not finished");
            }

            return "";
        }

        private static RangeParseState NextStateAfterChar(RangeParseState currentState)
        {
            return currentState == RangeParseState.CharExpectedAfterDash
                ? RangeParseState.CharExpected
                : RangeParseState.CharOrDashExpected;
        }

        private enum RangeParseState
        {
            CharExpected,
            CharOrDashExpected,
            CharExpectedAfterDash
        }
    }
}
