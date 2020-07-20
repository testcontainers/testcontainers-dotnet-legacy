using System.Runtime.InteropServices;

namespace TestContainers
{
    public static class EnvironmentHelper
    {
        public static bool IsWindows() =>
#if NET45
            true;
#else
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif

        public static bool IsOSX() =>
#if NET45
            false;
#else
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#endif

        public static bool IsLinux() =>
#if NET45
            false;
#else
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#endif
    }
}
