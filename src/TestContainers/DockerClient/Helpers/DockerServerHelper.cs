using System;
using System.Collections.Generic;
using System.Text;

namespace TestContainers
{
    public static class DockerServerHelper
    {
        static string osType = string.Empty;
        static string OsType()
        {
            if (string.IsNullOrWhiteSpace(osType))
            {
                using (var client = DockerClientFactory.Instance.Client())
                {
                    osType = client.System.GetSystemInfoAsync().GetAwaiter().GetResult().OSType;
                }
            }

            return osType;
        }

        public static bool IsLinux() => OsType() == "linux";
        public static bool IsWindows() => OsType() == "windows";
    }
}
