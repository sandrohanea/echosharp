// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Abstractions.SpeechTranscription;
using EchoSharp.Abstractions.VoiceActivityDetection;

namespace EchoSharp.SpeechTranscription;

/// <summary>
/// Default implementation for <see cref="IRealtimeSpeechTranscriptor"/> that uses a <seealso cref="ISpeechTranscriptor" and a <seealso cref="IVadDetector"/> to archive near-real-time processing .
/// </summary>
public class EchoSharpRealtimeTranscriptorFactory(
    ISpeechTranscriptorFactory speechTranscriptorFactory,
    IVadDetectorFactory vadDetectorFactory,
    ISpeechTranscriptorFactory? recognizingSpeechTranscriptorFactory = null,
    EchoSharpRealtimeOptions? echoSharpOptions = null,
    VadDetectorOptions? vadDetectorOptions = null)
    : IRealtimeSpeechTranscriptorFactory
{
    public IRealtimeSpeechTranscriptor Create(RealtimeSpeechTranscriptorOptions options)
    {
        var vadDetector = vadDetectorFactory.CreateVadDetector(vadDetectorOptions ?? new VadDetectorOptions());
        return new EchoSharpRealtimeTranscriptor(speechTranscriptorFactory, vadDetector, recognizingSpeechTranscriptorFactory, options, echoSharpOptions ?? new EchoSharpRealtimeOptions());
    }
}
