// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;
using EchoSharp.Abstractions.SpeechTranscription;
using Whisper.net;

namespace EchoSharp.Whisper.net;

public sealed class WhisperSpeechTranscriptorFactory : ISpeechTranscriptorFactory
{
    private readonly WhisperFactory? whisperFactory;
    private readonly WhisperProcessorBuilder builder;

    public WhisperSpeechTranscriptorFactory(WhisperFactory factory, bool dispose = true)
    {
        builder = factory.CreateBuilder();
        if (dispose)
        {
            whisperFactory = factory;
        }
    }

    public WhisperSpeechTranscriptorFactory(WhisperProcessorBuilder builder)
    {
        this.builder = builder;
    }

    public WhisperSpeechTranscriptorFactory(string modelFileName)
    {
        whisperFactory = WhisperFactory.FromPath(modelFileName);
        builder = whisperFactory.CreateBuilder();
    }

    public ISpeechTranscriptor Create(SpeechTranscriptorOptions options)
    {
        var currentBuilder = builder;
        if (options.Prompt != null)
        {
            currentBuilder = currentBuilder.WithPrompt(options.Prompt);
        }
        if (options.LanguageAutoDetect)
        {
            currentBuilder = currentBuilder.WithLanguage("auto");
        }
        else
        {
            currentBuilder = currentBuilder.WithLanguage(ToWhisperLanguage(options.Language));
        }

        if (options.RetrieveTokenDetails)
        {
            currentBuilder = currentBuilder.WithTokenTimestamps();
        }

        var processor = currentBuilder.Build();
        return new WhisperSpeechTranscriptor(processor);
    }

    private static string ToWhisperLanguage(CultureInfo languageCode)
    {
        if (!WhisperSupportedLanguage.IsSupported(languageCode))
        {
            throw new NotSupportedException($"The language provided as: {languageCode.ThreeLetterISOLanguageName} is not supported by Whisper.net.");
        }
        return languageCode.TwoLetterISOLanguageName;
    }

    public void Dispose()
    {
        whisperFactory?.Dispose();
    }
}
