// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Audio;

/// <summary>
/// Represents an audio source that is already completed.
/// </summary>
/// <remarks>
/// This is an utility wrapper when you need to use an IAwaitableAudioSource but you already have the audio source completed.
/// </remarks>
public sealed class CompletedAudioSource(IAudioSource audioSource) : IAwaitableAudioSource
{
    public bool IsFlushed => true;

    public TimeSpan Duration => audioSource.Duration;

    public TimeSpan TotalDuration => audioSource.TotalDuration;

    public uint SampleRate => audioSource.SampleRate;

    public long FramesCount => audioSource.FramesCount;

    public ushort ChannelCount => audioSource.ChannelCount;

    public bool IsInitialized => audioSource.IsInitialized;

    public ushort BitsPerSample => audioSource.BitsPerSample;

    public Task<int> CopyFramesAsync(Memory<byte> destination, long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default)
    {
        return audioSource.CopyFramesAsync(destination, startFrame, maxFrames, cancellationToken);
    }

    public void Dispose()
    {
        audioSource.Dispose();
    }

    public void Flush()
    {
        // Nothing to flush
    }

    public Task<Memory<byte>> GetFramesAsync(long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default)
    {
        return audioSource.GetFramesAsync(startFrame, maxFrames, cancellationToken);
    }

    public Task<Memory<float>> GetSamplesAsync(long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default)
    {
        return audioSource.GetSamplesAsync(startFrame, maxFrames, cancellationToken);
    }

    public Task WaitForInitializationAsync(CancellationToken cancellationToken)
    {
        // Already initialized
        return Task.CompletedTask;
    }

    public Task WaitForNewSamplesAsync(long sampleCount, CancellationToken cancellationToken)
    {
        // Already flushed
        return Task.CompletedTask;
    }

    public Task WaitForNewSamplesAsync(TimeSpan minimumDuration, CancellationToken cancellationToken)
    {
        // Already flushed
        return Task.CompletedTask;
    }
}
