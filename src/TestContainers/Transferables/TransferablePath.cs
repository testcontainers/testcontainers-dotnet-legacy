using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;
using TestContainers.Internal;

namespace TestContainers.Transferables
{
    /// <summary>
    /// Represents a file or folder as a transferable
    /// </summary>
    public class TransferablePath : ITransferable
    {
        private readonly string _path;

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">when path is null</exception>
        public TransferablePath(string path)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        /// <inheritdoc />
        /// <exception cref="FileNotFoundException">when file/folder does not exist</exception>
        public long GetSize()
        {
            return new FileInfo(_path).Length;
        }

        /// <inheritdoc />
        /// <exception cref="FileNotFoundException">when file/folder does not exist</exception>
        public async Task TransferToAsync(TarArchive tarArchive, string destinationPath, CancellationToken ct = default)
        {
            if (tarArchive == null)
            {
                throw new ArgumentNullException(nameof(tarArchive));
            }

            if (destinationPath == null)
            {
                throw new ArgumentNullException(nameof(destinationPath));
            }

            if (ct.IsCancellationRequested)
            {
                return;
            }

            // tar is a linux concept, so all paths should be in linux paths
            destinationPath = OS.NormalizePath(destinationPath, OS.LinuxDirectorySeparator);

            if (Directory.Exists(_path))
            {
                await Task.Run(() =>
                    {
                        var canonicalPath = Path.GetFullPath(_path);
                        var tarEntry = TarEntry.CreateEntryFromFile(canonicalPath);

                        // this is needed because SharpZipLib has a hack to remove current directory even
                        // if the path is an absolute path
                        var rootPath = canonicalPath;
                        if (canonicalPath.IndexOf(Directory.GetCurrentDirectory(), StringComparison.Ordinal) == 0)
                        {
                            rootPath = rootPath.Substring(Directory.GetCurrentDirectory().Length);
                        }

                        // there is an issue in SharpZipLib that trims the starting / from the
                        // tar entry, thus, any root path that starts with / will not match
                        tarArchive.RootPath = rootPath.TrimStart(Path.DirectorySeparatorChar);
                        tarArchive.PathPrefix = destinationPath;

                        tarArchive.WriteEntry(tarEntry, true);

                        tarArchive.RootPath = "";
                        tarArchive.PathPrefix = null;
                    }, ct)
                    .ConfigureAwait(false);
            }
            else
            {
                await Task.Run(() =>
                    {
                        var canonicalPath = Path.GetFullPath(_path);
                        var tarEntry = TarEntry.CreateEntryFromFile(canonicalPath);
                        tarEntry.Name = destinationPath;
                        tarArchive.WriteEntry(tarEntry, true);
                    }, ct)
                    .ConfigureAwait(false);
            }
        }
    }
}
