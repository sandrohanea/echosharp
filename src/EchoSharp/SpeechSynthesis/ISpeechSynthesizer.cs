// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Audio.Sink;

namespace EchoSharp.SpeechSynthesis;

public interface ISpeechSynthesizer : IDisposable
{
    /// <summary>
    /// Synthesizes the given speech segment and writes the audio data to the given audio sink.
    /// </summary>
    Task SynthesizeAsync(SpeechSegment speechSegment, IAudioSink audioSink, CancellationToken cancellationToken = default);
}
