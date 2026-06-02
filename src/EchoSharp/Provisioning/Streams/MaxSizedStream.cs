// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning.Streams;

/// <summary>
/// A stream that enforces a maximum size.
/// </summary>
/// <remarks>
/// This stream wraps another stream and enforces a maximum size on reads and writes.
/// It is useful for protecting against DoS attacks with Zip bombs or other large payloads.
/// Read attempts are allowed to ask for a small amount past <paramref name="maxSize"/> because APIs such as
/// <see cref="Stream.CopyToAsync(Stream)"/> request a fixed-size buffer to discover the end of the stream.
/// Actual bytes returned from the wrapped stream are still rejected if they exceed <paramref name="maxSize"/>.
/// </remarks>
public class MaxSizedStream(Stream source, long maxSize, long readAttemptTolerance = MaxSizedStream.DefaultReadAttemptTolerance) : Stream
{
    /// <summary>
    /// The default read-attempt tolerance, matching the current <see cref="Stream.CopyToAsync(Stream)"/> read buffer size.
    /// </summary>
    public const long DefaultReadAttemptTolerance = 128 * 1024;

    private readonly long maxReadAttemptSize = GetMaxReadAttemptSize(maxSize, readAttemptTolerance);
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

        if (requestedEndPos > maxReadAttemptSize)
        {
            throw new InvalidOperationException(
                $"Cannot read beyond offset {maxSize} plus read attempt tolerance {readAttemptTolerance} (requested to read until {requestedEndPos}).");
        }

        var readBytes = source.Read(buffer, offset, count);
        AddReadBytes(readBytes);
        return readBytes;
    }

#if NET8_0_OR_GREATER
    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var requestedEndPos = currentReadPosition + buffer.Length;

        if (requestedEndPos > maxReadAttemptSize)
        {
            throw new InvalidOperationException(
                $"Cannot read beyond offset {maxSize} plus read attempt tolerance {readAttemptTolerance} (requested to read until {requestedEndPos}).");
        }

        var readBytes = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        AddReadBytes(readBytes);
        return readBytes;
    }
#else
    public override async Task<int> ReadAsync(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var requestedEndPos = currentReadPosition + count;

        if (requestedEndPos > maxReadAttemptSize)
        {
            throw new InvalidOperationException(
                $"Cannot read beyond offset {maxSize} plus read attempt tolerance {readAttemptTolerance} (requested to read until {requestedEndPos}).");
        }

        var readBytes = await source.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        AddReadBytes(readBytes);
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

        source.Write(buffer, offset, count);
    }

    private static long GetMaxReadAttemptSize(long maxSize, long readAttemptTolerance)
    {
        if (maxSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size must be non-negative.");
        }

        if (readAttemptTolerance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(readAttemptTolerance), "Read attempt tolerance must be non-negative.");
        }

        if (maxSize > long.MaxValue - readAttemptTolerance)
        {
            return long.MaxValue;
        }

        return maxSize + readAttemptTolerance;
    }

    private void AddReadBytes(int readBytes)
    {
        currentReadPosition += readBytes;

        if (currentReadPosition > maxSize)
        {
            throw new InvalidOperationException(
                $"Cannot read beyond offset {maxSize} (actually read until {currentReadPosition}).");
        }
    }
}
