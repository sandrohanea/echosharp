// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;
using System.Runtime.InteropServices;
using EchoSharp.AzureAI.SpeechServices;
using EchoSharp.AzureAI.SpeechServices.FastTranscription;
using EchoSharp.AzureAI.SpeechServices.RealTime;
using EchoSharp.NAudio;
using EchoSharp.Onnx.Sherpa.SpeechTranscription;
using EchoSharp.Onnx.SileroVad;
using EchoSharp.Onnx.Whisper;
using EchoSharp.OpenAI.Whisper;
using EchoSharp.Provisioning;
using EchoSharp.SpeechTranscription;
using EchoSharp.WebRtc.WebRtcVadSharp;
using EchoSharp.Whisper.net;
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

var vadDetectorProvisioner = GetVadDetectorProvisioner("silero"); // OR "webrtc"
var speechTranscriptorProvisioner = GetSpeechTranscriptorProvisioner("whisper.net"); // OR "azure fast api" OR "openai whisper"

// Throw if not on Windows (NAudio MicrophoneInputSource is not supported on other platforms)
if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    throw new NotSupportedException("This example can only run on Windows, we are using NAudio to capture audio from the microphone.");
}

var micAudioSource = new MicrophoneInputSource(deviceNumber: 1);

var realTimeProvisioner = GetRealTimeProvisioner("azure", speechTranscriptorProvisioner, vadDetectorProvisioner);

var realTimeTranscriptorFactory = await realTimeProvisioner.ProvisionAsync();
var realTimeTranscriptor = realTimeTranscriptorFactory.Create(new RealtimeSpeechTranscriptorOptions()
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

IRealtimeSpeechTranscriptorProvisioner GetRealTimeProvisioner(string type, ISpeechTranscriptorProvisioner speechTranscriptorProvisioner, IVadDetectorProvisioner vadDetectorProvisioner)
{
    return type switch
    {
        "echo sharp" => GetEchoSharpTranscriptorProvisioner(speechTranscriptorProvisioner, vadDetectorProvisioner),
        "azure" => GetAzureAIRealtimeTranscriptorProvisioner(),
        _ => throw new NotSupportedException()
    };
}

IRealtimeSpeechTranscriptorProvisioner GetAzureAIRealtimeTranscriptorProvisioner()
{
    var speechConfig = new AzureSpeechServicesConfig()
    {
        SubscriptionKey = "your-azure-speech-service-api-key",
        Endpoint = new Uri("https://your-azure-cognitive-service.cognitiveservices.azure.com/")
    };
    var options = new AzureAIRealtimeTranscriptorOptions()
    {
        CandidateLanguages = [new CultureInfo("en-US"), new CultureInfo("ro-RO")] // Candidate languages to use for automatic language detection
    };
    return new AzureAIRealtimeTranscriptorProvisioner(speechConfig, options);
}

IRealtimeSpeechTranscriptorProvisioner GetEchoSharpTranscriptorProvisioner(ISpeechTranscriptorProvisioner speechTranscriptorProvisioner, IVadDetectorProvisioner vadDetectorProvisioner)
{
    var config = new EchoSharpRealtimeTranscriptorConfig()
    {
        Options = new EchoSharpRealtimeOptions()
        {
            ConcatenateSegmentsToPrompt = false // Flag to concatenate segments to prompt when new segment is recognized (for the whole session)
        }
    };
    return new EchoSharpRealtimeTranscriptorProvisioner(vadDetectorProvisioner, speechTranscriptorProvisioner, config);
}

ISpeechTranscriptorProvisioner GetSpeechTranscriptorProvisioner(string type)
{
    return type switch
    {
        "whisper.net" => GetWhisperTranscriptorProvisioner(),
        "azure fast api" => GetAzureAIFastTranscriptorProvisioner(),
        "openai whisper" => GetOpenAITranscriptorProvisioner(),
        "whisper onnx" => GetWhisperOnnxTranscriptorProvisioner(),
        "sherpa onnx" => GetSherpaOnnxTranscriptorProvisioner(),
        _ => throw new NotSupportedException()
    };
}

ISpeechTranscriptorProvisioner GetWhisperTranscriptorProvisioner()
{
    return new WhisperSpeechTranscriptorProvisioner(new WhisperSpeechTranscriptorConfig()
    {
        GgmlType = Whisper.net.Ggml.GgmlType.Tiny,
        OpenVinoEncoderModelPath = Path.Combine("models", "whisper.net", "openvino"),
        QuantizationType = Whisper.net.Ggml.QuantizationType.NoQuantization,
        ModelPath = Path.Combine("models", "whisper.net")
    });
}

ISpeechTranscriptorProvisioner GetAzureAIFastTranscriptorProvisioner()
{
    // Replace with your Azure Cognitive Services Speech Service endpoint and key (Get from here): https://azure.microsoft.com/en-us/products/ai-services/ai-speech
    var endpoint = new Uri("https://your-azure-cognitive-service.cognitiveservices.azure.com/");
    var key = "your-azure-speech-service-api-key";
    return new AzureAIFastTranscriptorProvisioner(new AzureSpeechServicesConfig()
    {
        Endpoint = endpoint,
        SubscriptionKey = key,
    });
}

ISpeechTranscriptorProvisioner GetOpenAITranscriptorProvisioner()
{
    var openAiApiKey = "your-openai-api-key";
    return new OpenAIWhisperSpeechTranscriporProvisioner(new OpenAiWhisperSpeechTranscriptorConfig()
    {
        ApiKey = openAiApiKey
    });
}

ISpeechTranscriptorProvisioner GetWhisperOnnxTranscriptorProvisioner()
{
    return new WhisperOnnxSpeechTranscriptorProvisioner(new WhisperOnnxSpeechTranscriptorConfig()
    {
        ModelPath = Path.Combine("models", "whisper.onnx"),
        ModelType = WhisperOnnxModelType.Tiny,
    });
}

ISpeechTranscriptorProvisioner GetSherpaOnnxTranscriptorProvisioner()
{
    return new SherpaOnnxSpeechTranscriptorProvisioner(new SherpaOnnxSpeechTranscriptorConfig()
    {
        ModelPath = Path.Combine("models", "sherpa"),
        Model = SherpaOnnxModels.ZipFormerGigaSpeechInt8
    });
}

IVadDetectorProvisioner GetVadDetectorProvisioner(string vad)
{
    return vad switch
    {
        "silero" => GetSileroVadDetectorProvisioner(),
        "webrtc" => GetWebRtcVadSharpDetectorProvisioner(),
        _ => throw new NotSupportedException()
    };
}

IVadDetectorProvisioner GetWebRtcVadSharpDetectorProvisioner()
{
    return new WebRtcVadSharpDetectorProvisioner(new WebRtcVadSharpConfig()
    {
        OperatingMode = OperatingMode.HighQuality, // The operating mode of the VAD. The default is OperatingMode.HighQuality.
    });
}

IVadDetectorProvisioner GetSileroVadDetectorProvisioner()
{
    return new SileroVadProvisioner(new SileroVadConfig()
    {
        ModelPath = Path.Combine("models", "silero_vad.onnx"),
        Threshold = 0.5f, // The threshold for Silero VAD. The default is 0.5f.
        ThresholdGap = 0.15f, // The threshold gap for Silero VAD. The default is 0.15f.
    });
}
