// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.VoiceActivityDetection;

namespace EchoSharp.SpeechProcessing;

public class EchoSharpRealtimeProcessorConfig
{
    public EchoSharpRealtimeOptions? Options { get; set; }
    public VadDetectorOptions? VadOptions { get; set; }
}
