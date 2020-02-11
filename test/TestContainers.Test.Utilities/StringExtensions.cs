using System;

namespace Container.Test.Utility
{
    public static class StringExtensions
    {
        public static string TrimEndNewLine(this string input)
        {
            return input.TrimEnd(Environment.NewLine.ToCharArray());
        }
    }
}
