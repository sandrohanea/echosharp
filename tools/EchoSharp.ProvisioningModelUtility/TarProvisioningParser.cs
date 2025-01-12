// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.CompilerServices;
using System.Text;
using EchoSharp.Provisioning;
using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Unarchive;
using EchoSharp.SharpZipLib;
using ICSharpCode.SharpZipLib.Tar;

namespace EchoSharp.ProvisioningModelUtility;

internal class TarProvisioningParser(IHasher hasher, Stream source) : ProvisioningParserBase(hasher, source)
{
    public override ProvisioningModel.ArchiveTypes GetArchiveType()
    {
        return ProvisioningModel.ArchiveTypes.TarGz;
    }

    protected override async IAsyncEnumerable<UnarchiveFileEntry> EnumerateEntriesAsync(Stream stream, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var zipArchive = new TarInputStream(stream, Encoding.UTF8);

        do
        {
            var entry = await zipArchive.GetNextEntryAsync(cancellationToken);
            if (entry == null)
            {
                break;
            }
            yield return new TarSessionEntry(entry, zipArchive);
        } while (true);
    }
}
