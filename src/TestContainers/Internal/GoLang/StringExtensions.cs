namespace TestContainers.Internal.GoLang
{
    /// <summary>
    /// Extensions used by GoLangFileMatch
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Java implementation of substring uses endIndex as the second parameter instead of length
        /// To simply only copy the java implementation, we need to emulate this behaviour
        /// </summary>
        /// <param name="value">original string</param>
        /// <param name="startIndex">start of the substring, inclusive</param>
        /// <param name="endIndex">end of the substring, exclusive</param>
        /// <returns>the substring</returns>
        public static string SubstringByIndexes(this string value, int startIndex, int endIndex)
        {
            int length = endIndex - startIndex;
            return value.Substring(startIndex, length);
        }
    }
}
