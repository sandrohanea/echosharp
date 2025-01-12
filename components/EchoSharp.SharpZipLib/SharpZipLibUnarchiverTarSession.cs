// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.CompilerServices;
using System.Text;
using EchoSharp.Provisioning;
using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Unarchive;
using ICSharpCode.SharpZipLib.Tar;

namespace EchoSharp.SharpZipLib;

internal partial class SharpZipLibUnarchiverTarSession(IHasher hasher, Stream source, UnarchiverOptions options, Func<Stream, Stream>? decompressProvider) : UnarchiverSessionBase(hasher, source, options)
{
    protected override async IAsyncEnumerable<UnarchiveFileEntry> EnumerateEntriesAsync(Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var decompress = decompressProvider != null ? decompressProvider(stream) : stream;
        using var tarArchive = new TarInputStream(stream, Encoding.UTF8);
        do
        {
            var entry = await tarArchive.GetNextEntryAsync(cancellationToken);
            if (entry == null)
            {
                break;
            }
            yield return new TarSessionEntry(entry, tarArchive);
        } while (true);

    }
}
