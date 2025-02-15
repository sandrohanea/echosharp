// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Collections.Concurrent;
using System.Security.Cryptography;
using EchoSharp.Provisioning.Streams;

namespace EchoSharp.Provisioning.Hasher;

public class Sha512Hasher : IHasher
{
    private readonly ConcurrentBag<HashAlgorithm> sha512Pool = [];

    public static Sha512Hasher Instance { get; } = new Sha512Hasher();

    public async Task<string> ComputeHashAsync(string file, CancellationToken cancellationToken)
    {
        if (!sha512Pool.TryTake(out var sha512))
        {
            sha512 = SHA512.Create();
        }
        else
        {
            sha512.Initialize();
        }
        try
        {
            using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
#if NET8_0_OR_GREATER
            var hash = await sha512.ComputeHashAsync(fileStream, cancellationToken);
#else
            var hash = sha512.ComputeHash(fileStream);
            // This is a workaround for the lack of ComputeHashAsync in .NET Standard
            await Task.CompletedTask;
#endif
            return Convert.ToBase64String(hash);
        }
        finally
        {
            sha512Pool.Add(sha512);
        }
    }

    public HasherStream CreateStream(Stream source, string? expectedHash)
    {
        if (!sha512Pool.TryTake(out var sha512))
        {
            sha512 = SHA512.Create();
        }
        else
        {
            sha512.Initialize();
        }

        return new HasherStream(source, sha512, sha512Pool, expectedHash);
    }

    public StringHasher CreateStringHasher()
    {
        if (!sha512Pool.TryTake(out var sha512))
        {
            sha512 = SHA512.Create();
        }
        else
        {
            sha512.Initialize();
        }
        return new StringHasher(sha512, sha512Pool);
    }
}
