// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Buffers;
using EchoSharp.Abstractions.Audio;
using EchoSharp.Abstractions.Config;

namespace EchoSharp.Audio;

public abstract class PcmStreamSource(Stream pcmStream, bool leaveOpen = true, int bufferLength = PcmStreamSource.DefaultBufferLength, IChannelAggregationStrategy? aggregationStrategy = null)
    : IAudioSource
{
    public const int DefaultBufferLength = 16 * 1024;

    private bool isDisposed;

    public ushort ChannelCount => aggregationStrategy != null ? (ushort)1 : SourceChannelCount;

    public abstract TimeSpan Duration { get; }

    public abstract TimeSpan TotalDuration { get; }

    public abstract uint SampleRate { get; }

    public abstract long FramesCount { get; }

    public abstract bool IsInitialized { get; }

    public abstract ushort BitsPerSample { get; }

    public virtual int SourceFrameSize => BitsPerSample * SourceChannelCount / 8;

    protected abstract ushort SourceChannelCount { get; }

    protected abstract int DataChunkOffset { get; }
    protected Stream PcmStream => pcmStream;

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }
        Dispose(true);
        GC.SuppressFinalize(this);
        isDisposed = true;
    }

    public async Task<int> CopyFramesAsync(Memory<byte> destination, long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default)
    {
        await InitializeIfNotReady(cancellationToken);
        await InitializeAsync(cancellationToken);
        EnsureFramePosition(startFrame);

        var totalFramesAvailable = Math.Min(FramesCount - startFrame, maxFrames);
        var totalBytesToRead = (int)(totalFramesAvailable * SourceFrameSize);

#if NET8_0_OR_GREATER
        if (aggregationStrategy == null)
        {
            var bytesToRead = Math.Min(totalBytesToRead, destination.Length);
            var bytesRead = await pcmStream.ReadAsync(destination.Slice(0, bytesToRead), cancellationToken);
            return bytesRead / SourceFrameSize;
        }
#endif

        var buffer = ArrayPool<byte>.Shared.Rent(totalBytesToRead);
        try
        {
            var bytesRead = await ReadAndAggregateAsync(totalBytesToRead, buffer.AsMemory(0, totalBytesToRead), cancellationToken);
            buffer.AsMemory(0, bytesRead).CopyTo(destination);
            return bytesRead / SourceFrameSize;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, ArrayPoolConfig.ClearOnReturn);
        }
    }

    public async Task<Memory<byte>> GetFramesAsync(long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default)
    {
        await InitializeIfNotReady(cancellationToken);
        EnsureFramePosition(startFrame);

        var totalFramesAvailable = Math.Min(FramesCount - startFrame, maxFrames);
        var totalBytesToRead = (int)(totalFramesAvailable * SourceFrameSize);
        var buffer = new byte[totalBytesToRead];

        var bytesRead = await ReadAndAggregateAsync(totalBytesToRead, buffer, cancellationToken);
        return bytesRead > 0 ? buffer.AsMemory(0, bytesRead) : Memory<byte>.Empty;
    }

    public async Task<Memory<float>> GetSamplesAsync(long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default)
    {
        await InitializeIfNotReady(cancellationToken);
        EnsureFramePosition(startFrame);

        var buffer = ArrayPool<byte>.Shared.Rent(bufferLength);
        var samples = new float[bufferLength];
        var indexSamples = 0;

        try
        {
            while (maxFrames > 0)
            {
                var framesToRead = Math.Min(maxFrames, bufferLength / SourceFrameSize);
                if (framesToRead <= 0)
                {
                    break;
                }

                var bytesToRead = framesToRead * SourceFrameSize;

#if NET8_0_OR_GREATER
                var bytesRead = await pcmStream.ReadAsync(buffer.AsMemory(0, bytesToRead), cancellationToken);
#else
                var bytesRead = await pcmStream.ReadAsync(buffer, 0, bytesToRead, cancellationToken);
#endif

                if (bytesRead == 0)
                {
                    break;
                }

                var samplesNeeded = indexSamples + bytesRead / SourceFrameSize;
                if (samplesNeeded > samples.Length)
                {
                    Array.Resize(ref samples, samplesNeeded * 2);
                }

                var framesProcessed = ConvertBufferToSamples(buffer.AsMemory(0, bytesRead), samples, indexSamples);
                indexSamples += framesProcessed;
                maxFrames -= framesProcessed;

                if (bytesRead < bytesToRead)
                {
                    break;
                }
            }

            return samples.AsMemory(0, indexSamples);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer, ArrayPoolConfig.ClearOnReturn);
        }
    }

    private async Task InitializeIfNotReady(CancellationToken cancellationToken)
    {
        if (!IsInitialized)
        {
            await InitializeAsync(cancellationToken);
        }
    }

    public abstract Task InitializeAsync(CancellationToken cancellationToken = default);

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (!leaveOpen)
            {
                pcmStream.Dispose();
            }
        }
    }

    private int ConvertBufferToSamples(ReadOnlyMemory<byte> buffer, float[] samples, int indexSamples)
    {
        if (aggregationStrategy != null)
        {
            var framesProcessed = 0;
            for (var i = 0; i < buffer.Length; i += SourceFrameSize)
            {
                aggregationStrategy.Aggregate(buffer.Slice(i, SourceFrameSize), samples.AsMemory(indexSamples++), BitsPerSample);
                framesProcessed++;
            }
            return framesProcessed;
        }
        else
        {
            var framesProcessed = (int)(buffer.Length * 8L / BitsPerSample);
            SampleSerializer.Deserialize(buffer, samples.AsMemory(indexSamples, framesProcessed), BitsPerSample);
            return framesProcessed;
        }
    }

    private async Task<int> ReadAndAggregateAsync(int bytesToRead, Memory<byte> buffer, CancellationToken cancellationToken)
    {
#if NET8_0_OR_GREATER
        var bytesRead = await pcmStream.ReadAsync(buffer.Slice(0, bytesToRead), cancellationToken);
#else
        var tempBuffer = new byte[bytesToRead];
        var bytesRead = await pcmStream.ReadAsync(tempBuffer, 0, bytesToRead, cancellationToken);
        tempBuffer.CopyTo(buffer);
#endif

        if (aggregationStrategy != null && bytesRead > 0)
        {
            return AggregateBuffer(buffer, bytesRead);
        }

        return bytesRead;
    }

    private int AggregateBuffer(Memory<byte> buffer, int bytesRead)
    {
        var indexAggregated = 0;
        var indexBuffer = 0;

        while (indexBuffer < bytesRead)
        {
            aggregationStrategy!.Aggregate(buffer.Slice(indexBuffer, SourceFrameSize), buffer.Slice(indexAggregated), BitsPerSample);
            indexAggregated += BitsPerSample / 8;
            indexBuffer += SourceFrameSize;
        }

        return indexAggregated;
    }

    private void EnsureFramePosition(long startFrame)
    {
        var expectedPosition = DataChunkOffset + startFrame * SourceFrameSize;
        if (pcmStream.Position != expectedPosition)
        {
            if (!pcmStream.CanSeek)
            {
                throw new InvalidOperationException("The stream does not support seeking");
            }

            pcmStream.Seek(expectedPosition, SeekOrigin.Begin);
        }
    }
}
