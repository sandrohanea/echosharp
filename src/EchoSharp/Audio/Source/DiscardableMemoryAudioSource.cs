// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Audio;

/// <summary>
/// Represents an audio source that can be discard samples at the beginning of the stream if not needed.
/// </summary>
/// <remarks>
/// It can store samples as floats or as bytes, or both. By default, it stores samples as floats.
/// Based on your usage, you can choose to store samples as bytes, floats, or both.
/// If storing them as floats, they will be deserialized from bytes when they are added and returned directly when requested.
/// If storing them as bytes, they will be serialized from floats when they are added and returned directly when requested.
/// If you want to optimize your memory usage, you can store them in the same format as they are added.
/// If you want to optimize your CPU usage, you can store them in the format you want to use them in.
/// </remarks>
public class DiscardableMemoryAudioSource(bool storeSamples = true,
                                          bool storeBytes = false,
                                          int initialSizeFloats = BufferedMemoryAudioSource.DefaultInitialSize,
                                          int initialSizeBytes = BufferedMemoryAudioSource.DefaultInitialSize,
                                          IChannelAggregationStrategy? aggregationStrategy = null)
    : BufferedMemoryAudioSource(storeSamples, storeBytes, initialSizeFloats, initialSizeBytes, aggregationStrategy), IDiscardableAudioSource
{
    private long framesDiscarded;

    public override long FramesCount => base.FramesCount - framesDiscarded;

    public long SampleVirtualCount => base.FramesCount;

    public override TimeSpan Duration => TimeSpan.FromMilliseconds(SampleVirtualCount * 1000d / SampleRate);

    /// <inheritdoc />
    public virtual void DiscardFrames(int count)
    {
        ThrowIfNotInitialized();

        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "readingBufferLength must be positive.");
        }
        if (count > FramesCount)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Cannot discard more samples than available.");
        }

        var framesToKeep = FramesCount - count;
        if (FloatFrames != null)
        {
            var samplesToDiscard = count * ChannelCount;
            var samplesToKeep = framesToKeep * ChannelCount;
            Array.Copy(FloatFrames, samplesToDiscard, FloatFrames, 0, samplesToKeep);
        }

        if (ByteFrames != null)
        {
            var bytesToDiscard = count * FrameSize;
            var bytesToKeep = framesToKeep * FrameSize;
            Array.Copy(ByteFrames, bytesToDiscard, ByteFrames, 0, bytesToKeep);
        }

        framesDiscarded += count;
    }

    /// <inheritdoc />
    public override Task<Memory<float>> GetSamplesAsync(long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfLessThan(startFrame, framesDiscarded, nameof(startFrame));
#else
        if (startFrame < framesDiscarded)
        {
            throw new ArgumentOutOfRangeException(nameof(startFrame), $"The startFrame must be greater or equal than the number of discarded frames: {framesDiscarded}");
        }
#endif
        return base.GetSamplesAsync(startFrame - framesDiscarded, maxFrames, cancellationToken);
    }

    /// <inheritdoc />
    public override Task<Memory<byte>> GetFramesAsync(long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfLessThan(startFrame, framesDiscarded, nameof(startFrame));
#else
        if (startFrame < framesDiscarded)
        {
            throw new ArgumentOutOfRangeException(nameof(startFrame), $"The startFrame must be greater or equal than the number of discarded frames: {framesDiscarded}");
        }
#endif
        return base.GetFramesAsync(startFrame - framesDiscarded, maxFrames, cancellationToken);
    }

    public override Task<int> CopyFramesAsync(Memory<byte> destination, long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfLessThan(startFrame, framesDiscarded, nameof(startFrame));
#else
        if (startFrame < framesDiscarded)
        {
            throw new ArgumentOutOfRangeException(nameof(startFrame), $"The startFrame must be greater or equal than the number of discarded frames: {framesDiscarded}");
        }
#endif
        return base.CopyFramesAsync(destination, startFrame - framesDiscarded, maxFrames, cancellationToken);
    }
}
