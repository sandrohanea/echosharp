// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning.Unarchive;

public interface IUnarchiver
{
    /// <summary>
    /// Creates an unarchiver session
    /// </summary>
    IUnarchiverSession CreateSession(UnarchiverOptions unarchiverOptions);
}
