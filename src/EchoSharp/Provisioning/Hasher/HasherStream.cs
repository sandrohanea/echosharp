// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Buffers;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace EchoSharp.Provisioning.Hasher;

public class HasherStream(Stream source, HashAlgorithm hashAlgorithm, ConcurrentBag<HashAlgorithm> poolHash, string? expectedHash) : Stream
{
    private const int hashBufferLength = 8192;
    private readonly CryptoStream cryptoStream = new(Null, hashAlgorithm, CryptoStreamMode.Write);

    /// <summary>
    /// How many bytes from the underlying stream have been hashed so far.
    /// </summary>
    private long hashPosition;

    /// <summary>
    /// If the user seeks backwards, we do not "un-hash".  
    /// Instead, we skip hashing again until the read position crosses <c>skipHashUntil</c>.
    /// </summary>
    private long skipHashUntil;

    private string? computedHash;
    public override bool CanRead => source.CanRead;
    public override bool CanSeek => source.CanSeek;
    public override bool CanWrite => false;
    public override long Length => source.Length;

    public override long Position
    {
        get => source.Position;
        set => Seek(value, SeekOrigin.Begin);
    }

    public override void Flush()
    {
        // Not relevant for a read-only hasher stream
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        // 1) Read from the underlying source
        var bytesRead = source.Read(buffer, offset, count);
        if (bytesRead == 0)
        {
            // End of stream -> finalize the hash
            ComputeAndVerifyHash();
            return 0;
        }

        // 2) Handle which part should be hashed vs. skipped
        HandleReadData(buffer, offset, bytesRead);

        // 3) If we didn't fill the user buffer, it's likely end-of-stream -> finalize hash
        if (bytesRead < count)
        {
            ComputeAndVerifyHash();
        }

        return bytesRead;
    }

#if NET8_0_OR_GREATER
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        var bytesRead = await source.ReadAsync(buffer, cancellationToken);
        if (bytesRead == 0)
        {
            ComputeAndVerifyHash();
            return 0;
        }

        // We can process the span version directly
        HandleReadData(buffer.Span, bytesRead);

        if (bytesRead < buffer.Length)
        {
            ComputeAndVerifyHash();
        }
        return bytesRead;
    }
#else
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var bytesRead = await source.ReadAsync(buffer, offset, count, cancellationToken);
        if (bytesRead == 0)
        {
            ComputeAndVerifyHash();
            return 0;
        }

        HandleReadData(buffer, offset, bytesRead);

        if (bytesRead < count)
        {
            ComputeAndVerifyHash();
        }
        return bytesRead;
    }
#endif

    /// <summary>
    /// Applies the skip/hash logic to the bytes just read. 
    /// We have the actual number of <paramref name="bytesRead"/> from the underlying source 
    /// (starting from Position - bytesRead up to Position).
    /// </summary>
    /// <remarks>
    /// This overload is for array segments; see the Memory/Span overload below for .NET 8.
    /// </remarks>
    private void HandleReadData(byte[] buffer, int offset, int bytesRead)
    {
        var readStartPos = Position - bytesRead; // after reading from source, Position advanced
        var readEndPos = Position;

        if (readEndPos <= skipHashUntil)
        {
            // completely below the region we still need to add to the hash
            return;
        }

        if (readStartPos >= skipHashUntil)
        {
            // Entire read is new data to be hashed
            cryptoStream.Write(buffer, offset, bytesRead);
            hashPosition += bytesRead;
            return;
        }

        // The read partially crosses skipHashUntil
        var alreadyHashedCount = skipHashUntil - readStartPos; // portion below skipHashUntil
        var newBytesCount = (int)(bytesRead - alreadyHashedCount);
        var newBytesOffset = offset + (int)alreadyHashedCount;

        cryptoStream.Write(buffer, newBytesOffset, newBytesCount);
        hashPosition += newBytesCount;
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// This overload works for .NET 8's <c>ReadAsync(Memory&lt;byte&gt;...)</c>.
    /// </summary>
    private void HandleReadData(Span<byte> buffer, int bytesRead)
    {
        var readStartPos = Position - bytesRead;
        var readEndPos = Position;

        if (readEndPos <= skipHashUntil)
        {
            // completely below the region we still need to add to the hash
            return;
        }

        if (readStartPos >= skipHashUntil)
        {
            // Entire read is new data to be hashed
            cryptoStream.Write(buffer[..bytesRead]);
            hashPosition += bytesRead;
            return;
        }

        // Partial
        var alreadyHashedCount = skipHashUntil - readStartPos;
        var newBytesCount = (int)(bytesRead - alreadyHashedCount);
        var newSegment = buffer.Slice((int)alreadyHashedCount, newBytesCount);

        cryptoStream.Write(newSegment);
        hashPosition += newBytesCount;
    }
#endif

    public override long Seek(long offset, SeekOrigin origin)
    {
        var newAbsolutePos = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => Length + offset,
            _ => throw new ArgumentException($"Invalid SeekOrigin: {origin}")
        };

        if (newAbsolutePos < 0)
        {
            throw new IOException("Cannot seek before beginning of stream.");
        }

        var oldPos = source.Position; // Where we are *before* this Seek
        if (newAbsolutePos == oldPos)
        {
            // No move => no hashing or skipping needed
            return oldPos;
        }

        // If we are seeking *backwards* (or to a lower pos) than we've hashed,
        // just skip future hashing until we pass hashPosition again.
        // Then do exactly one Seek to newAbsolutePos.
        if (newAbsolutePos < hashPosition)
        {
            skipHashUntil = hashPosition;
            source.Seek(newAbsolutePos, SeekOrigin.Begin); // One Seek
            return newAbsolutePos;
        }

        // Otherwise, newAbsolutePos >= hashPosition => we must "catch up" 
        // from whichever is bigger: the oldPos or the hashPosition, 
        // all the way up to newAbsolutePos.

        // We only do one Seek total. If we're *not* already at oldPos, 
        // we must Seek there. Then we read forward in one pass. 
        // However, if the source.Position is already `oldPos`, we can skip this.
        if (source.Position != oldPos)
        {
            source.Seek(oldPos, SeekOrigin.Begin);
        }

        // We'll read from `oldPos` to `newAbsolutePos`, hashing only the part 
        // from `hashPosition` onward. If oldPos < hashPosition, we skip 
        // the initial chunk. If oldPos > hashPosition, we hash from oldPos up.
        var bytesToReadTotal = newAbsolutePos - oldPos;
        if (bytesToReadTotal < 0)
        {
            throw new InvalidOperationException("Logic error: newAbsolutePos < oldPos but we didn't handle that earlier.");
        }

        var totalReadSoFar = 0;

        var buffer = ArrayPool<byte>.Shared.Rent(hashBufferLength);
        try
        {
            while (totalReadSoFar < bytesToReadTotal)
            {
                var toRead = (int)Math.Min(hashBufferLength, bytesToReadTotal - totalReadSoFar);
                var read = source.Read(buffer, 0, toRead);
                if (read <= 0)
                {
                    ComputeAndVerifyHash();
                    break; // EOF or no data
                }

                // The chunk we just read spans [oldPos + totalReadSoFar, oldPos + totalReadSoFar + read).
                var chunkStart = oldPos + totalReadSoFar;
                var chunkEnd = chunkStart + read;

                totalReadSoFar += read;

                // Now feed the hasher only the portion >= hashPosition
                if (chunkEnd > hashPosition)
                {
                    // Entire chunk is below the region we need to hash => skip
                    continue;
                }

                if (chunkStart >= hashPosition)
                {
                    // Entire chunk is new => hash everything
                    cryptoStream.Write(buffer, 0, read);
                    hashPosition += read;
                    continue;
                }

                // Partial overlap
                // The portion from [chunkStart ... hashPosition) is below 
                // the needed region, so skip it. The portion from 
                // [hashPosition ... chunkEnd) is new => hash it.
                var skipCount = hashPosition - chunkStart;
                var newBytesOffset = (int)skipCount;
                var newBytesCount = read - newBytesOffset;
                cryptoStream.Write(buffer, newBytesOffset, newBytesCount);
                hashPosition += newBytesCount;
            }

            // By now, the underlying `source.Position` should be at oldPos + totalReadSoFar. 
            // We want it to be exactly newAbsolutePos. 
            // If we read short (EOF?), we might not get that far. 
            // But if the stream has enough data, we are effectively at newAbsolutePos 
            // with no extra Seek call.

            return source.Position;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
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
            throw new HasherException(
                $"The source stream is not the expected one (expected: {expectedHash}, got: {computedHash}).");
        }
    }
}
