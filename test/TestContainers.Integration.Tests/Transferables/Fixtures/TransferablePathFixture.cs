using System;
using System.IO;
using System.Threading.Tasks;
using TestContainers.Internal;
using Xunit;

namespace TestContainers.Integration.Tests.Transferables.Fixtures
{
    public class TransferablePathFixture : IAsyncLifetime
    {
        private static readonly Random Random = new Random();

        public string TempFolderPath { get; }

        public string TempFilePath { get; }

        public long TempFileLengthInBytes { get; }

        public TransferablePathFixture()
        {
            TempFolderPath = Path.GetTempPath() + "/" + Random.NextAlphaNumeric(32);
            TempFilePath = Path.GetTempFileName();
            TempFileLengthInBytes = Random.Next(1, 9999);
        }

        public Task InitializeAsync()
        {
            var content = new byte[TempFileLengthInBytes];
            Random.NextBytes(content);

            File.WriteAllBytes(TempFilePath, content);

            var nestedTempDirectory = Directory.CreateDirectory(TempFolderPath + "/dummy");
            File.WriteAllBytes(nestedTempDirectory + "/temp.1", content);
            File.WriteAllBytes(TempFolderPath + "/temp.2", content);

            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            File.Delete(TempFilePath);
            Directory.Delete(TempFolderPath, true);
            return Task.CompletedTask;
        }
    }
}
