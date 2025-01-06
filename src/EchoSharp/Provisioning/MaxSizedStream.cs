// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning;

/// <summary>
/// A stream that enforces a maximum size.
/// </summary>
/// <remarks>
/// This stream wraps another stream and enforces a maximum size on reads and writes.
/// It is usefull for protecting agains DoS attacks with Zip bombs or other large payloads.
/// </remarks>
public class MaxSizedStream(Stream source, long maxSize) : Stream
{
    public override bool CanRead => source.CanRead;
    public override bool CanSeek => source.CanSeek;
    public override bool CanWrite => source.CanWrite;

    public override long Length => source.Length;

    public override long Position
    {
        get => source.Position;
        set => source.Position = value;
    }

    public override void Flush()
    {
        source.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        // Check if this read would cross our maxSize boundary.
        var currentPos = source.Position;
        var requestedEndPos = currentPos + count;

        if (requestedEndPos > maxSize)
        {
            // If *any* part of this read would exceed maxSize, we throw.
            // Alternatively, you could do a partial read up to maxSize - currentPos.
            throw new InvalidOperationException(
                $"Cannot read beyond offset {maxSize} (requested to read until {requestedEndPos}).");
        }

        return source.Read(buffer, offset, count);
    }

#if NET8_0_OR_GREATER
    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var currentPos = source.Position;
        var requestedEndPos = currentPos + buffer.Length;

        if (requestedEndPos > maxSize)
        {
            throw new InvalidOperationException(
                $"Cannot read beyond offset {maxSize} (requested to read until {requestedEndPos}).");
        }

        return await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
    }
#else
    public override async Task<int> ReadAsync(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var currentPos = source.Position;
        var requestedEndPos = currentPos + count;

        if (requestedEndPos > maxSize)
        {
            throw new InvalidOperationException(
                $"Cannot read beyond offset {maxSize} (requested to read until {requestedEndPos}).");
        }

        return await source.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
    }
#endif

    public override long Seek(long offset, SeekOrigin origin)
    {
        // For a minimal solution, simply pass through.
        // If you want to disallow seeking beyond maxSize, 
        // you could check here too and throw if the new position would exceed maxSize.
        return source.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        source.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (source.Position + count > maxSize)
        {
            throw new InvalidOperationException(
                $"Cannot write beyond offset {maxSize} (requested to write until {source.Position + count}).");
        }

        // If this is truly read-only, you could throw NotSupportedException here.
        // Otherwise, pass through to underlying.
        source.Write(buffer, offset, count);
    }
}
