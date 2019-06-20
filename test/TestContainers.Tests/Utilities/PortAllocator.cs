using System.Net;
using System.Net.Sockets;

namespace TestContainers.Tests.Utilities
{
    public static class PortAllocator
    {
        public static int AllocatePortNumber()
        {
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            var port = ((IPEndPoint) l.LocalEndpoint).Port;
            l.Stop();

            return port;
        }
    }
}
