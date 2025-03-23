// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Utils;

namespace EchoSharp.Audio.Sink;

/// <summary>
/// Represents an audio sink that discards all audio data.
/// </summary>
public sealed class NullAudioSink : IAudioSink
{
    public ValueTask DisposeAsync()
    {
        return EchoValueTask.Completed;
    }

    public Task Initialize(AudioHeader audioHeader, double? duration)
    {
        return Task.CompletedTask;
    }

    public Task WriteAsync(ReadOnlyMemory<float> samples, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
