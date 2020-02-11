using System.IO;
using System.Net;
using System.Text;

namespace TestContainers.Test.Utilities
{
    public static class HttpClientHelper
    {
        public static string MakeGetRequest(string url)
        {
            var request = WebRequest.Create(url);
            var response = request.GetResponse();

            using (var receiveStream = response.GetResponseStream())
            using (var reader = new StreamReader(receiveStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
