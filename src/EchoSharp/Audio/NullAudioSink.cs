// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Audio;

/// <summary>
/// Represents an audio sink that discards all audio data.
/// </summary>
public sealed class NullAudioSink : IAudioSink
{
    public void Dispose()
    {

    }

    public Task Initialize(AudioHeader audioHeader, double? duration)
    {
        return Task.CompletedTask;
    }

    public Task WriteAsync(ReadOnlyMemory<float> samples, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
