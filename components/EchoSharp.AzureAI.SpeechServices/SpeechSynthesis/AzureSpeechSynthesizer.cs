// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Audio;
using EchoSharp.AzureAI.SpeechServices.Internals;
using EchoSharp.SpeechSynthesis;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
namespace EchoSharp.AzureAI.SpeechServices.SpeechSynthesis;

internal sealed class AzureSpeechSynthesizer(SpeechConfig speechConfig, SpeechSynthesizerOptions options, AzureSpeechSynthesizerOptions azureOptions, AuthTokenHandler? authTokenHandler) : ISpeechSynthesizer
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public async Task SynthesizeAsync(SpeechSegment speechSegment, IAudioSink audioSink, CancellationToken cancellationToken)
    {
        using var pushAudioOutputStream = new PushAudioOutputStream(new AudioSinkPushAudioOutputStream(audioSink));
        using var audioConfig = AudioConfig.FromStreamOutput(pushAudioOutputStream);
        using var speechSynthesizer = new SpeechSynthesizer(speechConfig, audioConfig);
        await (authTokenHandler?.InitializeAsync(cancellationToken) ?? Task.CompletedTask);
        using var tokenLoader = new OptionalDisposable(authTokenHandler?.GetLoader(speechSynthesizer.Properties));

        return speechSynthesizer.SpeakTextAsync(speechSegment.Text, cancellationToken);
    }

    private class AudioSinkPushAudioOutputStream(IAudioSink audioSink) : PushAudioOutputStreamCallback
    {
        public override uint Write(byte[] dataBuffer)
        {
            throw new NotImplementedException();
        }
    }
}
