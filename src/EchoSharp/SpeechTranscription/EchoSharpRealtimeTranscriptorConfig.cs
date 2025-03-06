// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.VoiceActivityDetection;

namespace EchoSharp.SpeechTranscription;

public class EchoSharpRealtimeTranscriptorConfig
{
    public EchoSharpRealtimeOptions? Options { get; set; }
    public VadDetectorOptions? VadOptions { get; set; }
}
