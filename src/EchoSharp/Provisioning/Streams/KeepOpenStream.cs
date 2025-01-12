// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning.Streams;

/// <summary>
/// Represents a stream that wraps another stream and prevents it from being closed when the wrapper is disposed.
/// </summary>
public class KeepOpenStream(Stream source) : Stream
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
        return source.Read(buffer, offset, count);
    }

    public override void SetLength(long value)
    {
        source.SetLength(value);
    }

#if NET8_0_OR_GREATER
    public override ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return source.ReadAsync(buffer, cancellationToken);
    }
#else
    public override Task<int> ReadAsync(
        byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return source.ReadAsync(buffer, offset, count, cancellationToken);
    }
#endif

    public override long Seek(long offset, SeekOrigin origin)
    {
        return source.Seek(offset, origin);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        source.Write(buffer, offset, count);
    }

    protected override void Dispose(bool disposing)
    {
        // Do not dispose the source stream.
        base.Dispose(disposing);
    }
}
