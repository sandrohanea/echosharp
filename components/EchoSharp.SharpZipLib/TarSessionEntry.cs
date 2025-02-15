// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning.Streams;
using EchoSharp.Provisioning.Unarchive;
using ICSharpCode.SharpZipLib.Tar;

namespace EchoSharp.SharpZipLib;

public class TarSessionEntry(TarEntry tarEntry, TarInputStream tarInputStream) : UnarchiveFileEntry(tarEntry.IsDirectory, tarEntry.Name)
{
    public override Task<Stream> OpenReadAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<Stream>(new KeepOpenStream(tarInputStream));
    }
}
