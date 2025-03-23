// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Audio.Sink;

/// <summary>
/// Represents an audio sink that can receive audio data.
/// </summary>
public interface IAudioSink : IAsyncDisposable
{
    /// <summary>
    /// Initializes the audio sink with the audio header and the duration of the audio stream.
    /// </summary>
    /// <remarks>
    /// The duration is optional and can be null.
    /// </remarks>
    Task Initialize(AudioHeader audioHeader, double? duration);

    /// <summary>
    /// Writes audio data to the sink (append only).
    /// </summary>
    /// <remarks>
    /// The samples are written as a sequence of 32-bit floating-point values in the range [-1.0, 1.0].
    /// The number of samples must be a multiple of the number of channels.
    /// In order to use this method, the stream must be initialized first.
    /// </remarks>
    Task WriteAsync(ReadOnlyMemory<float> samples, CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes raw audio data to the sink (append only) (wav format)
    /// </summary>
    /// <remarks>
    /// The data is written as a sequence of bytes.
    /// The buytes should have wav header appended first.
    /// </remarks>
    Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);
}
