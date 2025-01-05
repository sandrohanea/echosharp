// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace EchoSharp.Provisioning.Hasher;

public class Sha512Hasher : IHasher
{
    private readonly ConcurrentBag<SHA512> sha512Pool = [];

    public static Sha512Hasher Instance { get; } = new Sha512Hasher();

    public IHasherSession CreateSession()
    {
        if (!sha512Pool.TryTake(out var sha512))
        {
            sha512 = SHA512.Create();
        }
        else
        {
            sha512.Initialize();
        }
        return new Sha512HasherSession(sha512, new CryptoStream(Stream.Null, sha512, CryptoStreamMode.Write), sha512Pool);
    }
}
