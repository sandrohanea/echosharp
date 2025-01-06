// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning.Hasher;

namespace EchoSharp.Provisioning.Unarchive;

/// <summary>
/// Unarchiver that does not unarchive anything (just copies the file).
/// </summary>
public class UnarchiverCopy : IUnarchiver
{
    public const string ModelName = "model.bin";

    public static readonly UnarchiverCopy Instance = new();

    public IUnarchiverSession CreateSession(IHasher hasher, Stream stream, UnarchiverOptions options)
    {
        return new UnarchiverCopySession(hasher, stream, options);
    }
}
