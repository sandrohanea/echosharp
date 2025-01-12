// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.CompilerServices;
using System.Text;
using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Streams;
using EchoSharp.Provisioning.Unarchive;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;

namespace EchoSharp.SharpZipLib;

internal class SharpZipLibUnarchiverZipSession(IHasher hasher, Stream source, UnarchiverOptions options, Func<Stream, Stream>? decompressProvider) : UnarchiverSessionBase(hasher, source, options)
{
    protected override async IAsyncEnumerable<UnarchiveFileEntry> EnumerateEntriesAsync(Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var decompress = decompressProvider != null ? decompressProvider(stream) : stream;
        using var zipInputStream = new ZipInputStream(stream, StringCodec.Default);
        do
        {
            var entry = zipInputStream.GetNextEntry();
            if (entry == null)
            {
                break;
            }
            yield return new ZipSessionEntry(entry, zipInputStream);
        } while (true);

        await Task.CompletedTask;
    }

    private class ZipSessionEntry(ZipEntry zipEntry, ZipInputStream zipInputStream) : UnarchiveFileEntry(zipEntry.IsDirectory, zipEntry.Name)
    {
        public override Task<Stream> OpenReadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<Stream>(new KeepOpenStream(zipInputStream));
        }
    }
}
