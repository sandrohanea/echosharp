// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Audio.Sink;

public class WaveFileSink : WaveEncoderSink
{
    private readonly Stream stream;
    private readonly bool leaveOpen;

    public WaveFileSink(string filePath)
    {
        stream = File.Create(filePath);
        leaveOpen = false;
    }

    public WaveFileSink(Stream stream, bool leaveOpen = false)
    {
        this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
        this.leaveOpen = leaveOpen;
    }

    public override async Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
#if NET8_0_OR_GREATER
        await stream.WriteAsync(buffer, cancellationToken);
#else
        await stream.WriteAsync(buffer.ToArray(), 0, buffer.Length, cancellationToken);
#endif
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        try
        {
            if (stream.CanSeek)
            {
                UpdateHeaderSize();
                stream.Position = 0;
#if NET8_0_OR_GREATER
                await stream.WriteAsync(headerBuffer.AsMemory(0, 44));
#else
                await stream.WriteAsync(headerBuffer, 0, 44);
#endif
                await stream.FlushAsync();
            }
        }
        finally
        {
            if (!leaveOpen)
            {
                stream.Dispose();
            }
        }
    }
}
