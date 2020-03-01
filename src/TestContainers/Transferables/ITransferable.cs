using System;
using System.Threading;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Tar;

namespace TestContainers.Transferables
{
    /// <summary>
    /// Represents an object that will be transferred to a destination
    /// </summary>
    public interface ITransferable
    {
        /// <summary>
        /// Gets the size of this transferable
        /// </summary>
        /// <returns>Size in bytes</returns>
        long GetSize();

        /// <summary>
        /// Transfers this transferable into the TarArchive
        /// </summary>
        /// <param name="tarArchive">Archive to transfer to</param>
        /// <param name="destinationPath">Path in the archive to transfer this transferable to</param>
        /// <param name="ct">Cancellation token</param>
        /// <exception cref="ArgumentNullException">when tarArchive or destinationPath is null</exception>
        Task TransferToAsync(TarArchive tarArchive, string destinationPath, CancellationToken ct = default);
    }
}
