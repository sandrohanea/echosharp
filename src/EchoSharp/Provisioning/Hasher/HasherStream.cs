// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace EchoSharp.Provisioning.Hasher;

public class HasherStream(Stream source, HashAlgorithm hashAlgorithm, ConcurrentBag<HashAlgorithm> poolHash, string? expectedHash = null) : Stream
{
    private string? computedHash;

    private readonly CryptoStream cryptoStream = new(Null, hashAlgorithm, CryptoStreamMode.Write);

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => source.Length;

    public override long Position { get => source.Position; set => source.Position = value; }

    public override void Flush()
    {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var result = cryptoStream.Read(buffer, offset, count);
        cryptoStream.Write(buffer, offset, count);
        if (result != count)
        {
            ComputeAndVerifyHash();
        }
        return result;
    }

#if NET8_0_OR_GREATER
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var result = await source.ReadAsync(buffer, cancellationToken);
        await cryptoStream.WriteAsync(buffer.Slice(0, result), cancellationToken);
        if (result != buffer.Length)
        {
            ComputeAndVerifyHash();
        }
        return result;
    }

#else
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var result = await source.ReadAsync(buffer, offset, count, cancellationToken);
        await cryptoStream.WriteAsync(buffer, offset, result);
        if (result != count)
        {
            ComputeAndVerifyHash();
        }
        return result;
    }
#endif

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            cryptoStream.Dispose();
            poolHash.Add(hashAlgorithm);
        }
        base.Dispose(disposing);
    }

    public string? ComputedHash => computedHash;

    private void ComputeAndVerifyHash()
    {
        if (hashAlgorithm.Hash == null)
        {
            throw new HasherException("Cannot compute the hash of the given stream");
        }

        computedHash = Convert.ToBase64String(hashAlgorithm.Hash);
        if (expectedHash is not null && expectedHash != computedHash)
        {
            throw new HasherException($"The source stream is not the expected one given the expected hash: {expectedHash}");
        }
    }
}
