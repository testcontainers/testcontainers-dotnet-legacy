using System;
using System.IO;

namespace TestContainers.Internal
{
    /// <summary>
    /// Static helper class for OS specific actions
    /// </summary>
    public static class OS
    {
        /// <summary>
        /// Windows directory separator char
        /// </summary>
        public const char WindowsDirectorySeparator = '\\';

        /// <summary>
        /// Linux directory separator char
        /// </summary>
        public const char LinuxDirectorySeparator = '/';

        /// <summary>
        /// Normalizes a path into its OS specific form
        /// It currently replaces directory separators
        /// </summary>
        /// <param name="path">path to normalize</param>
        /// <returns>normalized path</returns>
        /// <exception cref="NotSupportedException">when OS's directory separator is not supported</exception>
        public static string NormalizePath(string path)
        {
            return NormalizePath(path, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Normalizes a path to the desired form
        /// </summary>
        /// <param name="path">path to normalize</param>
        /// <param name="directorySeparator">desired directory separator to convert to</param>
        /// <returns>normalized path</returns>
        /// <exception cref="NotSupportedException">when desired directory separator is not supported</exception>
        public static string NormalizePath(string path, char directorySeparator)
        {
            char toReplace;
            char replacedTo;

            switch (directorySeparator)
            {
                case WindowsDirectorySeparator:
                    toReplace = LinuxDirectorySeparator;
                    replacedTo = WindowsDirectorySeparator;
                    break;
                case LinuxDirectorySeparator:
                    toReplace = WindowsDirectorySeparator;
                    replacedTo = LinuxDirectorySeparator;
                    break;
                default:
                    throw new NotSupportedException(
                        $"Directory separator[{directorySeparator}] is not a supported type");
            }

            return path.Replace(toReplace, replacedTo);
        }
    }
}
