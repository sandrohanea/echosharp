// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning;

public class ProvisioningModel(Uri uri, ProvisioningModel.ArchiveTypes archiveType, string archiveHash, string integrityHash, long archiveSize, long maxFileSize)
{
    public Uri Uri { get; } = uri;

    /// <summary>
    /// The base64 encoded sha512 hash of the archive file (as downloaded from the server).
    /// </summary>
    public string ArchiveHash { get; } = archiveHash;

    public long ArchiveSize { get; } = archiveSize;

    /// <summary>
    /// The type of archive.
    /// </summary>
    public ArchiveTypes ArchiveType { get; } = archiveType;

    /// <summary>
    /// The base64 encoded sha512 hash of the integrity file (a text file containing all the extracted files and their hashes).
    /// </summary>
    public string IntegrityHash { get; } = integrityHash;
    public long MaxFileSize { get; } = maxFileSize;

    public enum ArchiveTypes
    {
        /// <summary>
        /// No archive type, the model is not archived, and is a single file.
        /// </summary>
        None,
        /// <summary>
        /// The archive is a ZIP file with one or more files inside.
        /// </summary>
        Zip,

        TarBz2,

        TarGz,

        TarDeflate,

        /// <summary>
        /// The archive is another type of Archive, not supported by the default <seealso cref="EchoSharp.Provisioning.Unarchive.IUnarchiverSession"/>.
        /// </summary>
        Other
    }

}
