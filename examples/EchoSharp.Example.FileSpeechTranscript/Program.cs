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
using EchoSharp.Audio;
using EchoSharp.Audio.Source;
using EchoSharp.AzureAI.SpeechServices;
using EchoSharp.AzureAI.SpeechServices.FastTranscription;
using EchoSharp.NAudio;
using EchoSharp.Onnx.Sherpa.SpeechTranscription;
using EchoSharp.Onnx.SileroVad;
using EchoSharp.Onnx.Whisper;
using EchoSharp.OpenAI.Whisper;
using EchoSharp.Provisioning;
using EchoSharp.SpeechProcessing;
using EchoSharp.VoiceActivityDetection;
using EchoSharp.Whisper.net;

var transcritorFactory =
    await GetSpeechProcessorAsync(args.Length > 1 ? args[1] : "whisper.net"); // OR "azure fast api" OR "openai whisper"

// Optional chunking (split the audio to multiple chunks based on som IVadDetector and process them in parallel)
var vadProvisioner = new SileroVadProvisioner(new SileroVadConfig());
using var vadFactory = await vadProvisioner.ProvisionAsync();
using var vadDetector = vadFactory.CreateVadDetector(new VadDetectorOptions());
transcritorFactory = transcritorFactory.WithChunking(
    vadDetector,
    new ChunkedSpeechProcessorOptions()
    {
        MaxParallelism = 4, SegmentPadding = TimeSpan.FromMilliseconds(50),
    });

// Replace with the path to the audio file (the other example is files/testFile.wav)

PcmStreamSource pcmAudioSource = args.Length > 0 && args[0] == "mp3"
    ? new Mp3AudioSource("files/testFile.mp3",
        aggregationStrategy: DefaultChannelAggregationStrategies.SelectChannel(0))
    : new WaveFileAudioSource("files/testFile.wav");
await pcmAudioSource.InitializeAsync();

IAudioSource audioSource = pcmAudioSource.ChannelCount == 1 && pcmAudioSource.SampleRate == 16000
    ? pcmAudioSource
    : new ResamplerAudioSource(pcmAudioSource, 16000);

var transcriptor = transcritorFactory.Create(new SpeechProcessorOptions()
{
    Type = SpeechProcessingType.Transcript,
    LanguageAutoDetect = false, // Flag to auto-detect the language
    Language = new CultureInfo("en-US"), // Language to use for transcription
    RetrieveTokenDetails = true, // Flag to retrieve token details
    Prompt = null, // Prompt to use for transcription
});

await foreach (var segment in transcriptor.TranscribeAsync(audioSource, CancellationToken.None))
{
    Console.WriteLine($"{segment.StartTime}-{segment.StartTime + segment.Duration}:{segment.Text}");
}

Task<ISpeechProcessorFactory> GetSpeechProcessorAsync(string type)
{
    // Getting the provisioner (that is downloading the model)
    var provisioner = type switch
    {
        "whisper.net" => GetWhisperTranscriptorProvisioner(),
        "azure fast api" => GetAzureAIFastTranscriptorProvisioner(),
        "openai whisper" => GetOpenAITranscriptorProvisioner(),
        "whisper onnx" => GetWhisperOnnxTranscriptorProvisioner(),
        "sherpa onnx" => GetSherpaOnnxTranscriptorProvisioner(),
        _ => throw new NotSupportedException()
    };

    return provisioner.ProvisionAsync();
}

ISpeechProcessorProvisioner GetWhisperTranscriptorProvisioner()
{
    return new WhisperSpeechProcessorProvisioner(new WhisperSpeechProcessorConfig()
    {
        GgmlType = Whisper.net.Ggml.GgmlType.SmallEn,
        OpenVinoEncoderModelPath = Path.Combine("models", "whisper.net", "openvino"),
        QuantizationType = Whisper.net.Ggml.QuantizationType.NoQuantization,
        ModelPath = Path.Combine("models", "whisper.net")
    });
}

ISpeechProcessorProvisioner GetWhisperOnnxTranscriptorProvisioner()
{
    return new WhisperOnnxSpeechProcessorProvisioner(new WhisperOnnxSpeechProcessorConfig()
    {
        ModelPath = Path.Combine("models", "whisper.onnx"), ModelType = WhisperOnnxModelType.Tiny,
    });
}

ISpeechProcessorProvisioner GetAzureAIFastTranscriptorProvisioner()
{
    // Replace with your Azure Cognitive Services Speech Service endpoint and key (Get from here): https://azure.microsoft.com/en-us/products/ai-services/ai-speech
    var endpoint = new Uri("https://your-azure-cognitive-service.cognitiveservices.azure.com/");
    var key = "your-azure-speech-service-api-key";
    return new AzureAIFastTranscriptorProvisioner(new AzureSpeechServicesConfig()
    {
        Endpoint = endpoint, SubscriptionKey = key,
    });
}

ISpeechProcessorProvisioner GetOpenAITranscriptorProvisioner()
{
    var openAiApiKey = "your-openai-api-key";
    return new OpenAIWhisperSpeechTranscriporProvisioner(new OpenAiWhisperSpeechProcessorConfig()
    {
        ApiKey = openAiApiKey
    });
}

ISpeechProcessorProvisioner GetSherpaOnnxTranscriptorProvisioner()
{
    return new SherpaOnnxSpeechProcessorProvisioner(new SherpaOnnxSpeechProcessorConfig()
    {
        ModelPath = Path.Combine("models", "sherpa"), Model = SherpaOnnxModels.ZipFormerGigaSpeechInt8
    });
}
