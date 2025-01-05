// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace EchoSharp.Provisioning.Hasher;

internal class Sha512HasherSession(SHA512 sha512, CryptoStream cryptoStream, ConcurrentBag<SHA512> pool) : IHasherSession
{
    public void Dispose()
    {
        cryptoStream.Dispose();
        pool.Add(sha512);
    }

    public string GetBase64Hash()
    {
        cryptoStream.FlushFinalBlock();
        return Convert.ToBase64String(sha512.Hash!);
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Push more archive data which will be processed by the hasher.
    /// </summary>
    /// <returns></returns>
    public ValueTask PushAsync(Memory<byte> data, CancellationToken cancellationToken)
    {
        return cryptoStream.WriteAsync(data, cancellationToken);
    }
#else
    /// <summary>
    /// Push more archive data which will be processed by the hasher.
    /// </summary>
    /// <returns></returns>
    public Task PushAsync(byte[] data, int offset, int count, CancellationToken cancellationToken)
    {
        return cryptoStream.WriteAsync(data, offset, count, cancellationToken);
    }
#endif
}
