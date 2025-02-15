// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;
using EchoSharp.Audio;
using EchoSharp.SpeechTranscription;
using Xunit;
using EchoSharp.Whisper.net;
using EchoSharp.WebRtc.WebRtcVadSharp;
using EchoSharp.Onnx.SileroVad;
using SherpaOnnx;
using EchoSharp.Onnx.Sherpa.SpeechTranscription;
using FluentAssertions;
using EchoSharp.VoiceActivityDetection;

namespace EchoSharp.Tests.SpeechTranscription;

public class EchoSharpRealtimeTranscriptorTests
{
    [Theory]
    [InlineData("webrtc", "whisper")]
#if NET8_0_OR_GREATER
    // Onnx models are not supported on net472
    [InlineData("silero", "whisper")]
    [InlineData("webrtc", "sherpa")]
    [InlineData("silero", "sherpa")]
#endif
    public async Task RealTime_Integration_Test(string vadDetector, string transcriptor)
    {
        var waveSource = new AwaitableWaveFileSource(aggregationStrategy: DefaultChannelAggregationStrategies.Average);

        IVadDetectorFactory vadDetectorFactory;

        vadDetectorFactory = vadDetector == "webrtc"
            ? new WebRtcVadSharpDetectorFactory(new WebRtcVadSharpOptions())
            : new SileroVadDetectorFactory(new SileroVadOptions("./models/silero_vad.onnx"));

        var modelConfig = new OnlineModelConfig();
        var ctcFstDecoderConfig = new OnlineCtcFstDecoderConfig();

        modelConfig.Zipformer2Ctc.Model = "./models/sherpa-onnx-streaming-zipformer-ctc-small-2024-03-18/ctc-epoch-30-avg-3-chunk-16-left-128.int8.onnx";

        modelConfig.Tokens = "./models/sherpa-onnx-streaming-zipformer-ctc-small-2024-03-18/tokens.txt";
        modelConfig.Provider = "cpu";
        modelConfig.NumThreads = 1;
        modelConfig.Debug = 0;
        ctcFstDecoderConfig.Graph = "./models/sherpa-onnx-streaming-zipformer-ctc-small-2024-03-18/HLG.fst";

        ISpeechTranscriptorFactory transcritorFactory = transcriptor == "whisper"
            ? new WhisperSpeechTranscriptorFactory("./models/ggml-base.bin")
            : new SherpaOnnxSpeechTranscriptorFactory(new SherpaOnnxOnlineTranscriptorOptions()
            {
                OnlineModelConfig = modelConfig,
                OnlineCtcFstDecoderConfig = ctcFstDecoderConfig
            });

        var realTimeTranscriptorFactory = new EchoSharpRealtimeTranscriptorFactory(transcritorFactory, vadDetectorFactory);

        var readlTimeTranscriptor = realTimeTranscriptorFactory.Create(new RealtimeSpeechTranscriptorOptions()
        {
            LanguageAutoDetect = true,
            AutodetectLanguageOnce = true,
            Language = new CultureInfo("en-US"),
            IncludeSpeechRecogizingEvents = true,
        });

        var task1 = Process(readlTimeTranscriptor, waveSource);

        var file = "./files/testFile.wav";

        using var fileStream = File.OpenRead(file);

        while (true)
        {
            var buffer = new byte[9600];
            var bytesRead = fileStream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                break;
            }
            waveSource.WriteData(buffer.AsMemory(0, bytesRead));
            var result = await Task.WhenAny(Task.Delay(200), task1);
            if (result == task1)
            {
                break;
            }
        }

        waveSource.Flush();
        var events = await task1;

        // Assert
        var recognizingEvents = events.OfType<RealtimeSegmentRecognizing>().ToList();
        var recognizedEvents = events.OfType<RealtimeSegmentRecognized>().ToList();

        recognizingEvents.Should().HaveCountGreaterThanOrEqualTo(1);
        recognizedEvents.Should().HaveCountGreaterThanOrEqualTo(1);
        events.First().Should().BeOfType<RealtimeSessionStarted>();
        events.Last().Should().BeOfType<RealtimeSessionStopped>();
    }

    private static async Task<List<IRealtimeRecognitionEvent>> Process(IRealtimeSpeechTranscriptor transcriptor, IAwaitableAudioSource source)
    {
        var events = new List<IRealtimeRecognitionEvent>();
        await foreach (var @event in transcriptor.TranscribeAsync(source, CancellationToken.None))
        {
            events.Add(@event);
            Console.WriteLine(@event.GetType());
            if (@event is RealtimeSegmentRecognized recognizedEvent)
            {
                Console.WriteLine(recognizedEvent.Segment.Text);
            }
            if (@event is RealtimeSegmentRecognizing recognizing)
            {
                Console.WriteLine(recognizing.Segment.Text);
            }
        }
        return events;
    }
}

