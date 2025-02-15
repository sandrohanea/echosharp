// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.IO.Compression;
using System.Runtime.CompilerServices;
using EchoSharp.Provisioning;
using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Unarchive;

namespace EchoSharp.ProvisioningModelUtility;

internal class ZipProvisioningParser(IHasher hasher, Stream source) : ProvisioningParserBase(hasher, source)
{
    public override ProvisioningModel.ArchiveTypes GetArchiveType()
    {
        return ProvisioningModel.ArchiveTypes.Zip;
    }
    protected override async IAsyncEnumerable<UnarchiveFileEntry> EnumerateEntriesAsync(Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        foreach (var entry in zipArchive.Entries)
        {
            yield return new ZipSessionEntry(entry);
        }
        await Task.CompletedTask;
    }

    private class ZipSessionEntry(ZipArchiveEntry zipEntry) : UnarchiveFileEntry(string.IsNullOrEmpty(zipEntry.Name), zipEntry.FullName)
    {
        public override Task<Stream> OpenReadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(zipEntry.Open());
        }
    }
}
