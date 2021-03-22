using System;
using System.Text;

namespace TestContainers.Internal
{
    /// <summary>
    /// Extension methods for Random class
    /// </summary>
    public static class RandomExtensions
    {
        private const string AlphaNumericPool = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        /// <summary>
        /// Returns an alphanumeric string
        /// </summary>
        /// <param name="random">Random instance</param>
        /// <param name="length">Length of string to generate</param>
        /// <returns>An alphanumeric string of given length</returns>
        public static string NextAlphaNumeric(this Random random, int length)
        {
            return NextString(random, length, AlphaNumericPool);
        }

        private static string NextString(this Random random, int length, string pool)
        {
            var result = new StringBuilder(length);
            for (var i = 0; i < length; i++)
            {
                result.Append(pool[random.Next(pool.Length)]);
            }

            return result.ToString();
        }
    }
}
