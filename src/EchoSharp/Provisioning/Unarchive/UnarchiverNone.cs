// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning.Unarchive;

/// <summary>
/// Unarchiver that does not unarchive anything (just copies the file).
/// </summary>
public class UnarchiverNone : IUnarchiver
{
    public const string ModelName = "model.bin";

    public static readonly UnarchiverNone Instance = new();

    public IUnarchiverSession CreateSession(UnarchiverOptions options)
    {
        return new UnarchiverNoneSession(options);
    }
}
