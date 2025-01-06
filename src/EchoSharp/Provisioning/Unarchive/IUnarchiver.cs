// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning.Hasher;

namespace EchoSharp.Provisioning.Unarchive;

public interface IUnarchiver
{
    /// <summary>
    /// Creates an unarchiver session
    /// </summary>
    IUnarchiverSession CreateSession(IHasher hasher, Stream stream, UnarchiverOptions options);
}
