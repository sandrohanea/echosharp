// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning;

public class ProvisioningModel(string name, Uri uri, string archiveHash, string integrityHash, long archiveSize, long maxFileSize)
{
    public string Name { get; } = name;
    public Uri Uri { get; } = uri;
    public string ArchiveHash { get; } = archiveHash;

    /// <summary>
    /// The base64 encoded sha512 hash of the archive file (as downloaded from the server).
    /// </summary>
    public long ArchiveSize { get; } = archiveSize;

    /// <summary>
    /// The base64 encoded sha512 hash of the integrity file (a text file containing all the extracted files and their hashes).
    /// </summary>
    public string IntegrityHash { get; } = integrityHash;
    public long MaxFileSize { get; } = maxFileSize;
}
