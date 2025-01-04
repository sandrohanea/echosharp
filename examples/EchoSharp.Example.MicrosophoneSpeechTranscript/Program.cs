// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;
using Azure.Identity;
using EchoSharp.AzureAI.SpeechServices.FastTranscription;
using EchoSharp.AzureAI.SpeechServices.RealTime;
using EchoSharp.NAudio;
using EchoSharp.Onnx.Sherpa.SpeechTranscription;
using EchoSharp.Onnx.SileroVad;
using EchoSharp.Onnx.Whisper;
using EchoSharp.OpenAI.Whisper;
using EchoSharp.SpeechTranscription;
using EchoSharp.VoiceActivityDetection;
using EchoSharp.WebRtc.WebRtcVadSharp;
using EchoSharp.Whisper.net;
using SherpaOnnx;
using WebRtcVadSharp;

// This is an example that showcases how to use the EchoSharp library to transcribe speech in real-time using a microphone as input.
// The example demonstrates how to use the EchoSharpRealtimeTranscriptorFactory to create a real-time speech transcriptor that uses a microphone as input.
//
// It uses multiple EchoSharp Components:
//    - VAD: voice activity detection is performed using one of these components:
//            = EchoSharp.Onnx.SileroVad: based on Silero VAD ONNX: https://github.com/snakers4/silero-vad/
//         OR = EchoSharp.WebRtc.WebRtcVadSharp: based on WebRtcVadSharp: https://github.com/ladenedge/WebRtcVadSharp/
//
//    - Speech To Text:
//             = EchoSharp.Whisper.net: based on Whisper.net: https://github.com/sandrohanea/whisper.net
//          OR = EchoSharp.AzureAI.SpeechServices.FastTranscription: based on Azure Cognitive Services Speech Service: https://azure.microsoft.com/en-us/products/ai-services/ai-speech
//          OR = EchoSharp.OpenAI.Whisper: based on OpenAI Whisper: https://platform.openai.com/docs/guides/speech-to-text
//
//    - EchoSharp.NAudio: audio input is captured using this component
//
// Note: EchoSharp.Whisper.net, EchoSharp.Onnx.SileroVad and EchoSharp.WebRtc.WebRtcVadSharp can be run locally without any cloud dependencies.

var vadDetectorFactory = GetVadDetector("silero"); // OR "webrtc"
var speechTranscriptorFactory = GetSpeechTranscriptor("whisper.net"); // OR "azure fast api" OR "openai whisper"

var micAudioSource = new MicrophoneInputSource(deviceNumber: 1);

var realTimeFactory = GetRealTimeTranscriptorFactory("azure", speechTranscriptorFactory, vadDetectorFactory);

var realTimeTranscriptor = realTimeFactory.Create(new RealtimeSpeechTranscriptorOptions()
{
    AutodetectLanguageOnce = false, // Flag to detect the language only once or for each segment
    IncludeSpeechRecogizingEvents = true, // Flag to include speech recognizing events (RealtimeSegmentRecognizing)
    RetrieveTokenDetails = true, // Flag to retrieve token details
    LanguageAutoDetect = false, // Flag to auto-detect the language
    Language = new CultureInfo("en-US"), // Language to use for transcription
});

var microphoneTask = Task.Run(() =>
{
    micAudioSource.StartRecording();
    Console.WriteLine("Speak to recognize, press any key to stop...");
    Console.ReadKey();
    micAudioSource.StopRecording();
});

async Task ShowTranscriptAsync()
{

    await foreach (var transcription in realTimeTranscriptor.TranscribeAsync(micAudioSource))
    {
        var eventType = transcription.GetType().Name;
        Console.WriteLine(eventType);

        var textToWrite = transcription switch
        {
            RealtimeSegmentRecognized segmentRecognized => $"{segmentRecognized.Segment.StartTime}-{segmentRecognized.Segment.StartTime + segmentRecognized.Segment.Duration}:{segmentRecognized.Segment.Text}",
            RealtimeSegmentRecognizing segmentRecognizing => $"{segmentRecognizing.Segment.StartTime}-{segmentRecognizing.Segment.StartTime + segmentRecognizing.Segment.Duration}:{segmentRecognizing.Segment.Text}",
            RealtimeSessionStarted sessionStarted => $"SessionId: {sessionStarted.SessionId}",
            RealtimeSessionStopped sessionStopped => $"SessionId: {sessionStopped.SessionId}",
            _ => string.Empty
        };

        Console.WriteLine(textToWrite);
    }
};

var showTranscriptTask = ShowTranscriptAsync();
var firstReady = await Task.WhenAny(microphoneTask, showTranscriptTask);

// We await the task that finish first in case we have some exception to throw
await firstReady;

await Task.WhenAll(microphoneTask, showTranscriptTask);

IRealtimeSpeechTranscriptorFactory GetRealTimeTranscriptorFactory(string type, ISpeechTranscriptorFactory speechTranscriptorFactory, IVadDetectorFactory vadDetectorFactory)
{
    return type switch
    {
        "echo sharp" => GetEchoSharpTranscriptorFactory(speechTranscriptorFactory, vadDetectorFactory),
        "azure" => GetAzureAIRealtimeTranscriptorFactory(),
        _ => throw new NotSupportedException()
    };
}

IRealtimeSpeechTranscriptorFactory GetAzureAIRealtimeTranscriptorFactory()
{
    var azureRegion = "eastus"; // Replace with your Azure region
    var resourceId = "/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.CognitiveServices/accounts/{SpeechServiceAccount}";
    return new AzureAIRealtimeTranscriptorFactory(new DefaultAzureCredential(new DefaultAzureCredentialOptions()), resourceId, azureRegion, new AzureAIRealtimeTranscriptorOptions()
    {
        CandidateLanguages = [new CultureInfo("en-US"), new CultureInfo("ro-RO")] // Candidate languages to use for automatic language detection
    });
}

IRealtimeSpeechTranscriptorFactory GetEchoSharpTranscriptorFactory(ISpeechTranscriptorFactory speechTranscriptorFactory, IVadDetectorFactory vadDetectorFactory)
{
    return new EchoSharpRealtimeTranscriptorFactory(speechTranscriptorFactory, vadDetectorFactory, echoSharpOptions: new EchoSharpRealtimeOptions()
    {
        ConcatenateSegmentsToPrompt = false // Flag to concatenate segments to prompt when new segment is recognized (for the whole session)
    });
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
ISpeechTranscriptorFactory GetWhisperOnnxTranscriptor()
{
    // Replace with the path to the Whisper ONNX model (Download from here): https://huggingface.co/khmyznikov/whisper-int8-cpu-ort.onnx/tree/main
    // Or execute `downloadModels.ps1` script in the root of this repository
    var onnxPath = "models/whisper.onnx";
    return new WhisperOnnxSpeechTranscriptorFactory(onnxPath);
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

IVadDetectorFactory GetVadDetector(string vad)
{
    return vad switch
    {
        "silero" => GetSileroVadDetector(),
        "webrtc" => GetWebRtcVadSharpDetector(),
        _ => throw new NotSupportedException()
    };
}

IVadDetectorFactory GetWebRtcVadSharpDetector()
{
    return new WebRtcVadSharpDetectorFactory(new WebRtcVadSharpOptions()
    {
        OperatingMode = OperatingMode.HighQuality, // The operating mode of the VAD. The default is OperatingMode.HighQuality.
    });
}

IVadDetectorFactory GetSileroVadDetector()
{
    // Replace with the path to the Silero VAD ONNX model (Download from here): https://github.com/snakers4/silero-vad/blob/master/src/silero_vad/data/silero_vad.onnx
    // Or execute `downloadModels.ps1` script in the root of this repository
    var sileroOnnxPath = "models/silero_vad.onnx";
    return new SileroVadDetectorFactory(new SileroVadOptions(sileroOnnxPath)
    {
        Threshold = 0.5f, // The threshold for Silero VAD. The default is 0.5f.
        ThresholdGap = 0.15f, // The threshold gap for Silero VAD. The default is 0.15f.
    });
}
