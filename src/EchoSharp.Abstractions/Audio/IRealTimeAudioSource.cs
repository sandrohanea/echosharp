// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Abstractions.Audio;

public interface IRealTimeAudioSource : IAudioSource
{
    /// <summary>
    /// Gets a value indicating whether the stream is flushed and no more data will be written to it.
    /// </summary>
    bool IsFlushed { get; }

    /// <summary>
    /// Discards the specified number of samples from the beggining of the source.
    /// </summary>
    /// <remarks>
    /// Once the samples are discarded, they cannot be recovered and getting the samples from a discarded index will throw an exception.
    /// This method is useful when the samples were already processed, so they can be discarded to save memory.
    /// </remarks>
    /// <param name="sampleCount">The number of samples to discard.</param>
    void DiscardSamples(long sampleCount);

    /// <summary>
    /// Waits for the new samples to be available in the source. 
    /// </summary>
    /// <remarks>
    /// This task will complete when more than the specified number of samples are available in the source.
    /// </remarks>
    Task<int> WaitForNewSamplesAsync(long sampleCount, CancellationToken cancellationToken);

    /// <summary>
    /// Waits for the source to be initialized.
    /// </summary>
    Task WaitForInitializationAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Flushes the source and no more data will be written to it.
    /// </summary>
    void Flush();

    /// <summary>
    /// Adds the frame to the source.
    /// </summary>
    /// <param name="frame">The frame to be added with each channel as a float value.</param>
    void AddFrame(ReadOnlyMemory<float> frame);

    /// <summary>
    /// Adds the samples to the source.
    /// </summary>
    /// <param name="frame">The frame to be added with all channels interleaved and serialized as PCM.</param>
    void AddFrame(ReadOnlyMemory<byte> frame);

    /// <summary>
    /// Notifies the listeners that new samples are available in the source.
    /// </summary>
    void NotifyNewSamples();
}
