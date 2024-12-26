// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Abstractions.Audio;
using EchoSharp.Internals;

namespace EchoSharp.Audio;

/// <summary>
/// Represents an audio source that can be awaited for audio data.
/// </summary>
/// <remarks>
/// Important: This should be used at most by one writer and one reader.
/// It can store samples as floats or as bytes, or both. By default, it stores samples as floats.
/// Based on your usage, you can choose to store samples as bytes, floats, or both.
/// If storing them as floats, they will be deserialized from bytes when they are added and returned directly when requested.
/// If storing them as bytes, they will be serialized from floats when they are added and returned directly when requested.
/// If you want to optimize your memory usage, you can store them in the same format as they are added.
/// If you want to optimize your CPU usage, you can store them in the format you want to use them in.
/// </remarks>
public class AwaitableAudioSource(bool storeSamples = true,
                                  bool storeBytes = false,
                                  int initialSizeFloats = BufferedMemoryAudioSource.DefaultInitialSize,
                                  int initialSizeBytes = BufferedMemoryAudioSource.DefaultInitialSize,
                                  IChannelAggregationStrategy? aggregationStrategy = null)
    : DiscardableMemoryAudioSource(storeSamples, storeBytes, initialSizeFloats, initialSizeBytes, aggregationStrategy), IAwaitableAudioSource
{
#if NET9_0_OR_GREATER
    protected readonly Lock syncRoot = new();
#else
    protected readonly object syncRoot = new();
#endif
    private readonly AsyncAutoResetEvent samplesAvailableEvent = new();
    private readonly TaskCompletionSource<bool> initializationTcs = new();

    /// <summary>
    /// Gets a value indicating whether the source is flushed.
    /// </summary>
    public bool IsFlushed { get; private set; }

    public override Task<Memory<byte>> GetFramesAsync(long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default)
    {
        lock (syncRoot)
        {
            // Calling the base method with lock is fine here, as the base method will not await anything.
            return base.GetFramesAsync(startFrame, maxFrames, cancellationToken);
        }
    }

    public override Task<int> CopyFramesAsync(Memory<byte> destination, long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default)
    {
        lock (syncRoot)
        {
            // Calling the base method with lock is fine here, as the base method will not await anything.
            return base.CopyFramesAsync(destination, startFrame, maxFrames, cancellationToken);
        }
    }

    public override Task<Memory<float>> GetSamplesAsync(long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default)
    {
        lock (syncRoot)
        {
            // Calling the base method with lock is fine here, as the base method will not await anything.
            return base.GetSamplesAsync(startFrame, maxFrames, cancellationToken);
        }
    }

    public override void DiscardFrames(int count)
    {
        lock (syncRoot)
        {
            base.DiscardFrames(count);
        }
    }

    public override void AddFrame(ReadOnlyMemory<byte> frame)
    {
        lock (syncRoot)
        {
            if (IsFlushed)
            {
                throw new InvalidOperationException("The source is flushed and cannot accept new frames.");
            }
            base.AddFrame(frame);
            if (IsInitialized && !initializationTcs.Task.IsCompleted)
            {
                initializationTcs.SetResult(true);
            }
        }
    }

    public override void AddFrame(ReadOnlyMemory<float> frame)
    {
        lock (syncRoot)
        {
            if (IsFlushed)
            {
                throw new InvalidOperationException("The source is flushed and cannot accept new frames.");
            }
            base.AddFrame(frame);
            if (IsInitialized && !initializationTcs.Task.IsCompleted)
            {
                initializationTcs.SetResult(true);
            }
        }
    }

    /// <inheritdoc/>
    public async Task WaitForNewSamplesAsync(long sampleCount, CancellationToken cancellationToken)
    {
        while (!IsFlushed && SampleVirtualCount <= sampleCount)
        {
            await samplesAvailableEvent.WaitAsync().WaitWithCancellationAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task WaitForNewSamplesAsync(TimeSpan minimumDuration, CancellationToken cancellationToken)
    {
        while (!IsFlushed && Duration <= minimumDuration)
        {
            await samplesAvailableEvent.WaitAsync().WaitWithCancellationAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public async Task WaitForInitializationAsync(CancellationToken cancellationToken)
    {
        if (IsInitialized)
        {
            return;
        }

        await initializationTcs.Task.WaitWithCancellationAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Flush()
    {
        lock (syncRoot)
        {
            IsFlushed = true;
            samplesAvailableEvent.Set();
        }
    }

    /// <summary>
    /// Notifies that new samples are available.
    /// </summary>
    public void NotifyNewSamples()
    {
        samplesAvailableEvent.Set();
    }
}
