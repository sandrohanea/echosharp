// Licensed under the MIT license: https://opensource.org/licenses/MIT

// This is an example that showcases how to use the EchoSharp library to transcribe speech from a file
// The example demonstrates how to use the EchoSharpTranscriptorFactory to create a simple speech transcriptor that uses a file as an input.
//
// It uses multiple EchoSharp Components:
//    - Speech To Text:
//             = EchoSharp.Whisper.net: based on Whisper.net: https://github.com/sandrohanea/whisper.net
//          OR = EchoSharp.AzureAI.SpeechServices.FastTranscription: based on Azure Cognitive Services Speech Service: https://azure.microsoft.com/en-us/products/ai-services/ai-speech
//          OR = EchoSharp.OpenAI.Whisper: based on OpenAI Whisper: https://platform.openai.com/docs/guides/speech-to-text
//          OR = EchoSharp.Onnx.Whisper: based on Whisper for Onnx
//
//    - Audio Parsing:
//            = Simple Wav with PCM streams parsed by EchoSharp (no extra component required)
//            = EchoSharp.NAudio: mp3, ogg, wav, etc. parsed by NAudio:
//         
//
// Note: EchoSharp.Whisper.net, EchoSharp.Onnx.Whisper can be run locally without any cloud dependencies.

using System.Globalization;
using EchoSharp.Abstractions.Audio;
using EchoSharp.Abstractions.SpeechTranscription;
using EchoSharp.Audio;
using EchoSharp.AzureAI.SpeechServices.FastTranscription;
using EchoSharp.NAudio;
using EchoSharp.Onnx.Sherpa.SpeechTranscription;
using EchoSharp.Onnx.Whisper;
using EchoSharp.OpenAI.Whisper;
using EchoSharp.Whisper.net;
using SherpaOnnx;

var transcritorFactory = GetSpeechTranscriptor(args.Length > 1 ? args[1] : "whisper.net"); // OR "azure fast api" OR "openai whisper"

// Replace with the path to the audio file (the other example is files/testFile.wav)

PcmStreamSource pcmAudioSource = args.Length > 0 && args[0] == "mp3"
    ? new Mp3AudioSource("files/testFile.mp3", aggregationStrategy: DefaultChannelAggregationStrategies.SelectChannel(0))
    : new WaveFileAudioSource("files/testFile.wav");
await pcmAudioSource.InitializeAsync();

IAudioSource audioSource = pcmAudioSource.ChannelCount == 1 && pcmAudioSource.SampleRate == 16000 ? pcmAudioSource : new ResamplerAudioSource(pcmAudioSource, 16000);

var transcriptor = transcritorFactory.Create(new SpeechTranscriptorOptions()
{
    LanguageAutoDetect = false, // Flag to auto-detect the language
    Language = new CultureInfo("en-US"), // Language to use for transcription
    RetrieveTokenDetails = true, // Flag to retrieve token details
    Prompt = null, // Prompt to use for transcription
});

await foreach (var segment in transcriptor.TranscribeAsync(audioSource, CancellationToken.None))
{
    Console.WriteLine($"{segment.StartTime}-{segment.StartTime + segment.Duration}:{segment.Text}");
}

ISpeechTranscriptorFactory GetSpeechTranscriptor(string type)
{
    // Uncomment to use other speech transcriptor component
    return type switch
    {
        "whisper.net" => GetWhisperTranscriptor(),
        "azure fast api" => GetAzureAIFastTranscriptor(),
        "openai whisper" => GetOpenAITranscriptor(),
        "whisper onnx" => GetWhisperOnnxTranscriptor(),
        "sherpa onnx" => GetSherpaOnnxTranscriptor(),
        _ => throw new NotSupportedException()
    };
}

ISpeechTranscriptorFactory GetWhisperTranscriptor()
{
    // Replace with the path to the Whisper.net GGML model (Download from here): https://huggingface.co/sandrohanea/whisper.net/tree/main
    // Or execute `downloadModels.ps1` script in the root of this repository
    var ggmlModelPath = "models/ggml-base.bin";
    return new WhisperSpeechTranscriptorFactory(ggmlModelPath);
}

ISpeechTranscriptorFactory GetWhisperOnnxTranscriptor()
{
    // Replace with the path to the Whisper ONNX model (Download from here): https://huggingface.co/khmyznikov/whisper-int8-cpu-ort.onnx/tree/main
    // Or execute `downloadModels.ps1` script in the root of this repository
    var onnxPath = "models/whisper.onnx";
    return new WhisperOnnxSpeechTranscriptorFactory(onnxPath);
}

ISpeechTranscriptorFactory GetAzureAIFastTranscriptor()
{
    // Replace with your Azure Cognitive Services Speech Service endpoint and key (Get from here): https://azure.microsoft.com/en-us/products/ai-services/ai-speech
    var endpoint = new Uri("https://your-azure-cognitive-service.cognitiveservices.azure.com/");
    var key = "your-azure-speech-service-api-key";
    return new AzureAIFastTranscriptorFactory(endpoint, key);
}

ISpeechTranscriptorFactory GetOpenAITranscriptor()
{
    var openAiApiKey = "your-openai-api-key";
    return new OpenAIWhisperSpeechTranscriptorFactory(openAiApiKey);
}

ISpeechTranscriptorFactory GetSherpaOnnxTranscriptor()
{
    var modelConfig = new OnlineModelConfig();
    var ctcFstDecoderConfig = new OnlineCtcFstDecoderConfig();

    // Replace with your own model path (download from here: https://github.com/k2-fsa/sherpa-onnx/releases/tag/asr-models)
    modelConfig.Zipformer2Ctc.Model = "models/sherpa-onnx-streaming-zipformer-ctc-small-2024-03-18/ctc-epoch-30-avg-3-chunk-16-left-128.int8.onnx";

    modelConfig.Tokens = "models/sherpa-onnx-streaming-zipformer-ctc-small-2024-03-18/tokens.txt";
    modelConfig.Provider = "cpu";
    modelConfig.NumThreads = 1;
    modelConfig.Debug = 0;
    ctcFstDecoderConfig.Graph = "models/sherpa-onnx-streaming-zipformer-ctc-small-2024-03-18/HLG.fst";

    return new SherpaOnnxSpeechTranscriptorFactory(new SherpaOnnxOnlineTranscriptorOptions()
    {
        OnlineModelConfig = modelConfig,
        OnlineCtcFstDecoderConfig = ctcFstDecoderConfig
    });
}
