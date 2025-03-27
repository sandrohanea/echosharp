// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.VoiceActivityDetection;

namespace EchoSharp.SpeechProcessing;

/// <summary>
/// Default implementation for <see cref="IRealtimeSpeechProcessor"/> that uses a <seealso cref="ISpeechProcessor" and a <seealso cref="IVadDetector"/> to archive near-real-time processing .
/// </summary>
public class EchoSharpRealtimeProcessorFactory(
    ISpeechProcessorFactory speechTranscriptorFactory,
    IVadDetectorFactory vadDetectorFactory,
    ISpeechProcessorFactory? recognizingSpeechProcessorFactory = null,
    EchoSharpRealtimeOptions? echoSharpOptions = null,
    VadDetectorOptions? vadDetectorOptions = null)
    : IRealtimeSpeechProcessorFactory
{
    public IRealtimeSpeechProcessor Create(RealtimeSpeechProcessorOptions options)
    {
        // Will be disposed by the RealtimeSpeechProcessor
        var vadDetector = vadDetectorFactory.CreateVadDetector(vadDetectorOptions ?? new VadDetectorOptions());
        return new EchoSharpRealtimeProcessor(speechTranscriptorFactory, vadDetector, recognizingSpeechProcessorFactory, options, echoSharpOptions ?? new EchoSharpRealtimeOptions());
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}
