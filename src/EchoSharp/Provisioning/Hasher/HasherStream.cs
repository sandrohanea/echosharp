// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace EchoSharp.Provisioning.Hasher;

public class HasherStream(Stream source, HashAlgorithm hashAlgorithm, ConcurrentBag<HashAlgorithm> poolHash) : Stream
{
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
        throw new NotImplementedException();
    }

#if NET8_0_OR_GREATER
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var result = await source.ReadAsync(buffer, cancellationToken);
        await cryptoStream.WriteAsync(buffer.Slice(0, result), cancellationToken);
        return result;
    }

#else
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var result = await source.ReadAsync(buffer, offset, count, cancellationToken);
        await cryptoStream.WriteAsync(buffer, offset, result);
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

    public string GetBase64Hash()
    {
        return Convert.ToBase64String(hashAlgorithm.Hash!);
    }

}
