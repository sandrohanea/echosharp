// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;
using EchoSharp.SpeechProcessing;
using Whisper.net;

namespace EchoSharp.Whisper.net;

public sealed class WhisperSpeechProcessorFactory : ISpeechProcessorFactory
{
    private readonly WhisperFactory? whisperFactory;
    private readonly WhisperProcessorBuilder builder;
    private readonly Func<WhisperProcessorBuilder, WhisperProcessorBuilder>? builderConfig;

    public WhisperSpeechProcessorFactory(WhisperFactory factory, bool dispose = true)
    {
        builder = factory.CreateBuilder();
        if (dispose)
        {
            whisperFactory = factory;
        }
    }

    public WhisperSpeechProcessorFactory(WhisperProcessorBuilder builder)
    {
        this.builder = builder;
    }

    public WhisperSpeechProcessorFactory(string modelFileName, WhisperFactoryOptions whisperFactoryOptions, Func<WhisperProcessorBuilder, WhisperProcessorBuilder>? builderConfig = null)
    {
        this.builderConfig = builderConfig;
        whisperFactory = WhisperFactory.FromPath(modelFileName);
        builder = whisperFactory.CreateBuilder();
    }

    public WhisperSpeechProcessorFactory(Memory<byte> bufferMemory, WhisperFactoryOptions whisperFactoryOptions, Func<WhisperProcessorBuilder, WhisperProcessorBuilder>? builderConfig = null)
    {
        this.builderConfig = builderConfig;
        // TODO: Remove the array allocation once Whisper.net supports Memory<byte>
        whisperFactory = WhisperFactory.FromBuffer(bufferMemory.ToArray(), whisperFactoryOptions);
        builder = whisperFactory.CreateBuilder();
    }

    public WhisperSpeechProcessorFactory(string modelFileName, Func<WhisperProcessorBuilder, WhisperProcessorBuilder>? builderConfig = null)
    {
        this.builderConfig = builderConfig;
        whisperFactory = WhisperFactory.FromPath(modelFileName);
        builder = whisperFactory.CreateBuilder();
    }

    public WhisperSpeechProcessorFactory(Memory<byte> bufferMemory, Func<WhisperProcessorBuilder, WhisperProcessorBuilder>? builderConfig = null)
    {
        this.builderConfig = builderConfig;
        // TODO: Remove the array allocation once Whisper.net supports Memory<byte>
        whisperFactory = WhisperFactory.FromBuffer(bufferMemory.ToArray());
        builder = whisperFactory.CreateBuilder();
    }

    public ISpeechProcessor Create(SpeechProcessorOptions options)
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

        if (options.Type == SpeechProcessingType.Translate)
        {
            currentBuilder = currentBuilder.WithTranslate();
        }
        else if (options.Type != SpeechProcessingType.Transcript)
        {
            throw new NotSupportedException("Only Translate and Transcript processing options are supported by WhisperSpeechProcessor.");
        }

        if (builderConfig != null)
        {
            currentBuilder = builderConfig(currentBuilder);
        }

        var processor = currentBuilder.Build();
        return new WhisperSpeechProcessor(processor);
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
