// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning.Hasher;

namespace EchoSharp.Provisioning.Unarchive;

public class UnarchiverZip : IUnarchiver
{
    public static UnarchiverZip Instance { get; } = new UnarchiverZip();

    public IUnarchiverSession CreateSession(IHasher hasher, Stream stream, UnarchiverOptions options)
    {
        return new UnarchiverZipSession(hasher, stream, options);
    }
}
