// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Streams;
using EchoSharp.Provisioning.Unarchive;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;

namespace EchoSharp.SharpZipLib;

/// <summary>
/// An unarchiver powered by SharpZipLib library with support for multiple archive types
/// </summary>
public class SharpZipLibUnarchiver(SharpZipLibType type, Func<Stream, Stream>? decompressProvider) : IUnarchiver
{
    public static readonly SharpZipLibUnarchiver TarGz = new(SharpZipLibType.Tar, (stream) => new KeepOpenStream(new GZipInputStream(stream)));

    public static readonly SharpZipLibUnarchiver TarBz2 = new(SharpZipLibType.Tar, (stream) => new KeepOpenStream(new BZip2InputStream(stream)));

    public static readonly SharpZipLibUnarchiver Zip = new(SharpZipLibType.Zip, null);

    public IUnarchiverSession CreateSession(IHasher hasher, Stream stream, UnarchiverOptions options)
    {
        return type switch
        {
            SharpZipLibType.Tar => new SharpZipLibUnarchiverTarSession(hasher, stream, options, decompressProvider),
            SharpZipLibType.Zip => new SharpZipLibUnarchiverZipSession(hasher, stream, options, decompressProvider),
            _ => throw new NotSupportedException($"The archive type {type} is not supported.")
        };
    }
}
