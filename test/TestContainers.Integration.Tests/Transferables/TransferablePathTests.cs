using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;
using TestContainers.Integration.Tests.Transferables.Fixtures;
using TestContainers.Test.Utilities;
using TestContainers.Transferables;
using Xunit;

namespace TestContainers.Integration.Tests.Transferables
{
    [Collection(TransferablePathTestCollection.CollectionName)]
    public class TransferablePathTests
    {
        private readonly TransferablePathFixture _fixture;

        public TransferablePathTests(TransferablePathFixture fixture)
        {
            _fixture = fixture;
        }

        public class ConstructorTests : TransferablePathTests
        {
            public ConstructorTests(TransferablePathFixture fixture) : base(fixture)
            {
            }

            [Fact]
            public void ShouldThrowArgumentNullExceptionWhenPathIsNull()
            {
                // act
                var ex = Record.Exception(() => new TransferablePath(null));

                // assert
                Assert.IsType<ArgumentNullException>(ex);
            }
        }

        public class GetSizeTests : TransferablePathTests
        {
            public GetSizeTests(TransferablePathFixture fixture) : base(fixture)
            {
            }

            [Fact]
            public void ShouldReturnSizeOfFileIfItExists()
            {
                // arrange
                var mountableFile = new TransferablePath(_fixture.TempFilePath);

                // act
                var actual = mountableFile.GetSize();

                // assert
                Assert.Equal(_fixture.TempFileLengthInBytes, actual);
            }

            [Fact]
            public void ShouldThrowFileNotFoundExceptionIfFileDoesNotExist()
            {
                // arrange
                var mountableFile = new TransferablePath("/does/not/exist/path");

                // act
                var ex = Record.Exception(() => mountableFile.GetSize());

                // assert
                Assert.IsType<FileNotFoundException>(ex);
            }
        }

        public class TransferToTests : TransferablePathTests
        {
            private readonly TransferablePath _mountableFile;

            public TransferToTests(TransferablePathFixture fixture) : base(fixture)
            {
                _mountableFile = new TransferablePath(fixture.TempFilePath);
            }

            [Fact]
            public async Task ShouldThrowArgumentNullExceptionIfTarArchiveIsNull()
            {
                // act
                var ex = await Record.ExceptionAsync(async () => await _mountableFile.TransferToAsync(null, "my_file"));

                // assert
                Assert.IsType<ArgumentNullException>(ex);
            }

            [Fact]
            public async Task ShouldThrowArgumentNullExceptionIfDestinationIsNull()
            {
                // arrange
                var memoryStream = new MemoryStream();
                var tarArchive = TarArchive.CreateOutputTarArchive(memoryStream);

                // act
                var ex = await Record.ExceptionAsync(async () =>
                    await _mountableFile.TransferToAsync(tarArchive, null));

                // assert
                Assert.IsType<ArgumentNullException>(ex);
            }
        }

        public class TransferFromFileTests : TransferablePathTests
        {
            private readonly string _tarFilePath;
            private readonly FileStream _tarFileStream;
            private readonly TarArchive _tarWriteStream;

            public TransferFromFileTests(TransferablePathFixture fixture) : base(fixture)
            {
                _tarFilePath = Path.GetTempFileName();
                _tarFileStream = new FileStream(_tarFilePath, FileMode.Create);
                _tarWriteStream = TarArchive.CreateOutputTarArchive(_tarFileStream);
            }

            ~TransferFromFileTests()
            {
                _tarFileStream.Dispose();
                File.Delete(_tarFilePath);
            }

            [Fact]
            public async Task ShouldThrowFileNotFoundExceptionIfFileDoesNotExist()
            {
                // arrange
                var mountableFile = new TransferablePath("/does/not/exist/path");

                // act
                var ex = await Record.ExceptionAsync(async () =>
                    await mountableFile.TransferToAsync(_tarWriteStream, "no"));

                // assert
                Assert.IsType<FileNotFoundException>(ex);
            }

            [Fact]
            public async Task ShouldTransferRecursivelyToArchiveIfFolderExists()
            {
                // arrange
                var mountableFile = new TransferablePath(_fixture.TempFilePath);
                var destinationFileName = Path.GetFileName(_fixture.TempFilePath);

                // act
                await mountableFile.TransferToAsync(_tarWriteStream, destinationFileName);
                _tarWriteStream.Close();

                // assert
                using (var tarFileStream = new FileStream(_tarFilePath, FileMode.Open))
                using (var tarReadStream = TarArchive.CreateInputTarArchive(tarFileStream))
                {
                    var extractionPath = Path.GetTempPath() + "/extracted";
                    Directory.CreateDirectory(extractionPath);
                    tarReadStream.ExtractContents(extractionPath);

                    var expected = new FileInfo(_fixture.TempFilePath);
                    var actual = new FileInfo(extractionPath + "/" + destinationFileName);

                    Assert.Equal(expected, actual, new FileComparer());

                    Directory.Delete(extractionPath, true);
                }
            }
        }

        public class TransferFromFolderTests : TransferablePathTests
        {
            private readonly string _tarFilePath;
            private readonly FileStream _tarFileStream;
            private readonly TarArchive _tarWriteStream;

            public TransferFromFolderTests(TransferablePathFixture fixture) : base(fixture)
            {
                _tarFilePath = Path.GetTempFileName();
                _tarFileStream = new FileStream(_tarFilePath, FileMode.Create);
                _tarWriteStream = TarArchive.CreateOutputTarArchive(_tarFileStream);
            }

            ~TransferFromFolderTests()
            {
                _tarFileStream.Dispose();
                File.Delete(_tarFilePath);
            }

            [Fact]
            public async Task ShouldThrowFileNotFoundExceptionIfFolderDoesNotExist()
            {
                // arrange
                var mountableFile = new TransferablePath("/does/not/exist/path");

                // act
                var ex = await Record.ExceptionAsync(async () =>
                    await mountableFile.TransferToAsync(_tarWriteStream, "."));

                // assert
                Assert.IsType<FileNotFoundException>(ex);
            }

            [Fact]
            public async Task ShouldTransferRecursivelyToArchiveIfFolderExists()
            {
                // arrange
                var mountableFile = new TransferablePath(_fixture.TempFolderPath);

                // act
                await mountableFile.TransferToAsync(_tarWriteStream, ".");
                _tarWriteStream.Close();

                // assert
                using (var tarFileStream = new FileStream(_tarFilePath, FileMode.Open))
                using (var tarReadStream = TarArchive.CreateInputTarArchive(tarFileStream))
                {
                    var extractionPath = Path.GetTempPath() + "/extracted";
                    Directory.CreateDirectory(extractionPath);
                    tarReadStream.ExtractContents(extractionPath);

                    AssertDirectoryEquals(_fixture.TempFolderPath, extractionPath);

                    Directory.Delete(extractionPath, true);
                }
            }

            private static void AssertDirectoryEquals(string expectedPath, string actualPath)
            {
                var expectedDirectory = new DirectoryInfo(expectedPath);
                var actualDirectory = new DirectoryInfo(actualPath);

                var expectedFiles = expectedDirectory.GetFiles("*", SearchOption.AllDirectories);
                var actualFiles = actualDirectory.GetFiles("*", SearchOption.AllDirectories);

                var areIdentical = expectedFiles.SequenceEqual(actualFiles, new FileComparer());

                Assert.True((bool) areIdentical);
            }
        }
    }
}
