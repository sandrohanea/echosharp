// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;
using EchoSharp.SpeechTranscription;
using Whisper.net;
using Whisper.net.LibraryLoader;

namespace EchoSharp.Whisper.net;

public sealed class WhisperSpeechTranscriptorFactory : ISpeechTranscriptorFactory
{
    private readonly WhisperFactory? whisperFactory;
    private readonly WhisperProcessorBuilder builder;
    private readonly Func<WhisperProcessorBuilder, WhisperProcessorBuilder>? builderConfig;

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

    public WhisperSpeechTranscriptorFactory(string modelFileName, WhisperFactoryOptions whisperFactoryOptions, Func<WhisperProcessorBuilder, WhisperProcessorBuilder>? builderConfig = null)
    {
        this.builderConfig = builderConfig;
        whisperFactory = WhisperFactory.FromPath(modelFileName);
        builder = whisperFactory.CreateBuilder();
    }

    public WhisperSpeechTranscriptorFactory(Memory<byte> bufferMemory, WhisperFactoryOptions whisperFactoryOptions, Func<WhisperProcessorBuilder, WhisperProcessorBuilder>? builderConfig = null)
    {
        this.builderConfig = builderConfig;
        // TODO: Remove the array allocation once Whisper.net supports Memory<byte>
        whisperFactory = WhisperFactory.FromBuffer(bufferMemory.ToArray(), whisperFactoryOptions);
        builder = whisperFactory.CreateBuilder();
    }

    public WhisperSpeechTranscriptorFactory(string modelFileName, Func<WhisperProcessorBuilder, WhisperProcessorBuilder>? builderConfig = null)
    {
        this.builderConfig = builderConfig;
        whisperFactory = WhisperFactory.FromPath(modelFileName);
        builder = whisperFactory.CreateBuilder();
    }

    public WhisperSpeechTranscriptorFactory(Memory<byte> bufferMemory, Func<WhisperProcessorBuilder, WhisperProcessorBuilder>? builderConfig = null)
    {
        this.builderConfig = builderConfig;
        // TODO: Remove the array allocation once Whisper.net supports Memory<byte>
        whisperFactory = WhisperFactory.FromBuffer(bufferMemory.ToArray());
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

        if (builderConfig != null)
        {
            currentBuilder = builderConfig(currentBuilder);
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
