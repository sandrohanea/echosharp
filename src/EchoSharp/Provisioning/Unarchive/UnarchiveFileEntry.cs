// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning.Unarchive;

public abstract class UnarchiveFileEntry(bool isDirectory, string fullName)
{
    public bool IsDirectory { get; } = isDirectory;
    public string FullName { get; } = fullName;

    public abstract Task<Stream> OpenReadAsync(CancellationToken cancellationToken);
}
