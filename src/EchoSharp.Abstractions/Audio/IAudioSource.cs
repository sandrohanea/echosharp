// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Abstractions.Audio;

/// <summary>
/// Represents an audio source.
/// </summary>
public interface IAudioSource : IDisposable
{
    /// <summary>
    /// Gets the duration of the audio stream.
    /// </summary>
    TimeSpan Duration { get; }

    /// <summary>
    /// Gets the total duration of the audio stream.
    /// </summary>
    /// <remarks>
    /// TotalDuration can include virtual frames that are not available in the source.
    /// </remarks>
    TimeSpan TotalDuration { get; }

    /// <summary>
    /// Gets or sets the sample rate of the audio stream.
    /// </summary>
    uint SampleRate { get; }

    /// <summary>
    /// Gets the number of frames available in the stream
    /// </summary>
    /// <remarks>
    /// The total number of samples in the stream is equal to the number of frames multiplied by the number of channels.
    /// </remarks>
    long FramesCount { get; }

    /// <summary>
    /// Gets the actual number of channels in the source.
    /// </summary>
    /// <remarks>
    /// Note, that the actual number of channels may be different from the number of channels in the header if the source uses an aggregation strategy.
    /// </remarks>
    ushort ChannelCount { get; }

    /// <summary>
    /// Gets a value indicating whether the stream is initialized.
    /// </summary>
    bool IsInitialized { get; }

    ushort BitsPerSample { get; }

    /// <summary>
    /// Gets the memory slices for all the samples interleaved by channel.
    /// </summary>
    /// <param name="startFrame">The frame index of the samples to get.</param>
    /// <param name="maxFrames">Optional. The maximum length of the frames to get.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns></returns>
    /// <remarks>
    /// The maximum length of the returned memory is <seealso cref="ChannelCount"/> * <paramref name="maxFrames"/>.
    /// </remarks>
    Task<Memory<float>> GetSamplesAsync(long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the memory slices for all the frames interleaved by channel.
    /// </summary>
    /// <param name="startFrame">The frame index of the samples to get.</param>
    /// <param name="maxFrames">Optional. The maximum length of the frames to get.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns></returns>
    /// <remarks>
    /// The maximum length of the returned memory is <seealso cref="ChannelCount"/> * <paramref name="maxFrames"/> * <seealso cref="BitsPerSample"/> / 8.
    /// </remarks>
    Task<Memory<byte>> GetFramesAsync(long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default);

    Task<int> CopyFramesAsync(Memory<byte> destination, long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default);
}
