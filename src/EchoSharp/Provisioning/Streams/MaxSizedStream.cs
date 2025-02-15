// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning.Streams;

/// <summary>
/// A stream that enforces a maximum size.
/// </summary>
/// <remarks>
/// This stream wraps another stream and enforces a maximum size on reads and writes.
/// It is usefull for protecting agains DoS attacks with Zip bombs or other large payloads.
/// </remarks>
public class MaxSizedStream(Stream source, long maxSize) : Stream
{
    private long currentReadPosition;

    public override bool CanRead => source.CanRead;
    public override bool CanSeek => source.CanSeek;
    public override bool CanWrite => source.CanWrite;

    public override long Length => source.Length;

    public long CurrentReadSize => currentReadPosition;

    public override long Position
    {
        get => source.Position;
        set => Seek(value, SeekOrigin.Begin);
    }

    public override void Flush()
    {
        source.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        // Check if this read would cross our maxSize boundary.
        var requestedEndPos = currentReadPosition + count;

        if (requestedEndPos > maxSize)
        {
            // If *any* part of this read would exceed maxSize, we throw.
            // Alternatively, you could do a partial read up to maxSize - currentPos.
            throw new InvalidOperationException(
                $"Cannot read beyond offset {maxSize} (requested to read until {requestedEndPos}).");
        }

        var readBytes = source.Read(buffer, offset, count);
        currentReadPosition += readBytes;
        return readBytes;
    }

#if NET8_0_OR_GREATER
    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var requestedEndPos = currentReadPosition + buffer.Length;

        if (requestedEndPos > maxSize)
        {
            throw new InvalidOperationException(
                $"Cannot read beyond offset {maxSize} (requested to read until {requestedEndPos}).");
        }

        var readBytes = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        currentReadPosition += readBytes;
        return readBytes;
    }
#else
    public override async Task<int> ReadAsync(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var requestedEndPos = currentReadPosition + count;

        if (requestedEndPos > maxSize)
        {
            throw new InvalidOperationException(
                $"Cannot read beyond offset {maxSize} (requested to read until {requestedEndPos}).");
        }

        var readBytes = await source.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        currentReadPosition += readBytes;
        return readBytes;
    }
#endif

    public override long Seek(long offset, SeekOrigin origin)
    {
        currentReadPosition = source.Seek(offset, origin);
        return currentReadPosition;
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
