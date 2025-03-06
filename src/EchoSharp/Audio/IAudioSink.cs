// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Audio;

/// <summary>
/// Represents an audio sink that can receive audio data.
/// </summary>
public interface IAudioSink : IDisposable
{
    /// <summary>
    /// Initializes the audio sink with the audio header and the duration of the audio stream.
    /// </summary>
    /// <remarks>
    /// The duration is optional and can be null.
    /// </remarks>
    Task Initialize(AudioHeader audioHeader, double? duration);

    Task WriteAsync(ReadOnlyMemory<float> samples, CancellationToken cancellationToken = default);
}
