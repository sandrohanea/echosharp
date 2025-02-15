// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.IO.Compression;
using System.Runtime.CompilerServices;
using EchoSharp.Provisioning.Hasher;

namespace EchoSharp.Provisioning.Unarchive;

public class UnarchiverZipSession(IHasher hasher, Stream source, UnarchiverOptions options) : UnarchiverSessionBase(hasher, source, options)
{
    protected override async IAsyncEnumerable<UnarchiveFileEntry> EnumerateEntriesAsync(Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        foreach (var entry in zipArchive.Entries)
        {
            yield return new ZipSessionEntry(entry);
        }

        await Task.CompletedTask;
    }

    private class ZipSessionEntry(ZipArchiveEntry zipArchiveEntry) : UnarchiveFileEntry(string.IsNullOrEmpty(zipArchiveEntry.Name), zipArchiveEntry.FullName)
    {
        public override Task<Stream> OpenReadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(zipArchiveEntry.Open());
        }
    }
}
