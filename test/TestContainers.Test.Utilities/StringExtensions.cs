using System;

namespace TestContainers.Test.Utilities
{
    public static class StringExtensions
    {
        public static string TrimEndNewLine(this string input)
        {
            return input.TrimEnd(Environment.NewLine.ToCharArray());
        }
    }
}
