// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;
using EchoSharp.Audio;
using EchoSharp.Audio.Source.Awaitable;
using EchoSharp.SpeechProcessing;
using Xunit;

namespace EchoSharp.Tests.SpeechProcessing;

public class EchoSharpRealtimeProcessorFunctionalTests(FunctionalTestFixture fixture) : IClassFixture<FunctionalTestFixture>
{
    [Theory]
#if NET8_0_OR_GREATER
    // Onnx models are not supported on net472
    [InlineData("silero", "whisper")]
    [InlineData("silero", "sherpa")]
#endif
    public async Task RealTime_Integration_Test(string vadDetector, string transcriptor)
    {
        var waveSource = new AwaitableWaveFileSource(aggregationStrategy: DefaultChannelAggregationStrategies.Average);

        var vadDetectorFactory = vadDetector == "webrtc"
            ? await fixture.GetWebRtcVadDetectorFactoryAsync()
            : await fixture.GetSileroVadDetectorFactoryAsync();

        var speechProcessorFactory = transcriptor == "whisper"
            ? await fixture.GetWhisperProcessorFactoryAsync()
            : await fixture.GetSherpaProcessorFactoryAsync();

        var realTimeSpeechProcessorFactory = new EchoSharpRealtimeProcessorFactory(speechProcessorFactory, vadDetectorFactory);

        var realtimeSpeechProcessor = realTimeSpeechProcessorFactory.Create(new RealtimeSpeechProcessorOptions()
        {
            LanguageAutoDetect = true,
            AutodetectLanguageOnce = true,
            Language = new CultureInfo("en-US"),
            IncludeSpeechRecogizingEvents = true,
        });

        var task1 = Process(realtimeSpeechProcessor, waveSource);

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

        Assert.True(recognizingEvents.Count >= 1);
        Assert.True(recognizedEvents.Count >= 1);
        Assert.IsType<RealtimeSessionStarted>(events.First());
        Assert.IsType<RealtimeSessionStopped>(events.Last());
    }

    private static async Task<List<IRealtimeRecognitionEvent>> Process(IRealtimeSpeechProcessor transcriptor, IAwaitableAudioSource source)
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

