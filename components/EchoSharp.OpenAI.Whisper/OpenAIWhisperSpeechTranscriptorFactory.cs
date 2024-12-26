// Licensed under the MIT license: https://opensource.org/licenses/MIT

using OpenAI;
using OpenAI.Audio;
using EchoSharp.Abstractions.SpeechTranscription;

namespace EchoSharp.OpenAI.Whisper;

public sealed class OpenAIWhisperSpeechTranscriptorFactory : ISpeechTranscriptorFactory
{
    private readonly AudioClient audioClient;
    private readonly float? temperature;

    public OpenAIWhisperSpeechTranscriptorFactory(AudioClient audioClient, float? temperature = null)
    {
        this.audioClient = audioClient;
        this.temperature = temperature;
    }

    public OpenAIWhisperSpeechTranscriptorFactory(string apiKey, float? temperature = null)
    {
        var client = new OpenAIClient(apiKey);
        this.temperature = temperature;
        audioClient = client.GetAudioClient("whisper-1");
    }

    public ISpeechTranscriptor Create(SpeechTranscriptorOptions options)
    {
        var audioOptions = new AudioTranscriptionOptions()
        {
            Language = options.LanguageAutoDetect ? "auto" : options.Language.TwoLetterISOLanguageName,
            Prompt = options.Prompt,
            ResponseFormat = AudioTranscriptionFormat.Verbose, // TBD if we want to allow this to be configurable from the options
            Temperature = temperature,
            TimestampGranularities = options.RetrieveTokenDetails ? AudioTimestampGranularities.Word : AudioTimestampGranularities.Segment
        };

        return new OpenAIWhisperSpeechTranscriptor(audioClient, audioOptions);
    }

    public void Dispose()
    {
    }
}
