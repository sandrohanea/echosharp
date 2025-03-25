// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Audio.Sink;

namespace EchoSharp.SpeechSynthesis;

public static class SpeechSynthesizerExtensions
{
    public static Task SynthesizeAsync(ISpeechSynthesizer speechSynthesizer, string text, IAudioSink audioSink, CancellationToken cancellationToken = default)
    {
        return speechSynthesizer.SynthesizeAsync(new SpeechSegment(text), audioSink, cancellationToken);
    }
}
