// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.SpeechProcessing;
using OpenAI;
using OpenAI.Audio;

namespace EchoSharp.OpenAI.Whisper;

public sealed class OpenAIWhisperSpeechProcessorFactory : ISpeechProcessorFactory
{
    private readonly AudioClient audioClient;
    private readonly float? temperature;

    public OpenAIWhisperSpeechProcessorFactory(AudioClient audioClient, float? temperature = null)
    {
        this.audioClient = audioClient;
        this.temperature = temperature;
    }

    public OpenAIWhisperSpeechProcessorFactory(string apiKey, float? temperature = null)
    {
        var client = new OpenAIClient(apiKey);
        this.temperature = temperature;
        audioClient = client.GetAudioClient("whisper-1");
    }

    public ISpeechProcessor Create(SpeechProcessorOptions options)
    {
        if (options.Type != SpeechProcessingType.Transcript)
        {
            throw new NotSupportedException("Only Transcript processing is supported by OpenAIWhisperSpeechProcessor");
        }

        var audioOptions = new AudioTranscriptionOptions()
        {
            Language = options.LanguageAutoDetect ? "auto" : options.Language.TwoLetterISOLanguageName,
            Prompt = options.Prompt,
            ResponseFormat = AudioTranscriptionFormat.Verbose, // TBD if we want to allow this to be configurable from the options
            Temperature = temperature,
            TimestampGranularities = options.RetrieveTokenDetails ? AudioTimestampGranularities.Word : AudioTimestampGranularities.Segment
        };

        return new OpenAIWhisperSpeechProcessor(audioClient, audioOptions);
    }

    public void Dispose()
    {
    }
}
