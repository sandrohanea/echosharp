// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace EchoSharp.Provisioning.Hasher;

public sealed class StringHasher(HashAlgorithm hashAlgorithm, ConcurrentBag<HashAlgorithm> pool) : IDisposable
{
    public string GetBase64Hash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = hashAlgorithm.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public void Dispose()
    {
        pool.Add(hashAlgorithm);
    }
}
