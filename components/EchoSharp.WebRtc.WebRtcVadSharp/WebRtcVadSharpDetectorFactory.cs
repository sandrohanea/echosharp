// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Abstractions.VoiceActivityDetection;

namespace EchoSharp.WebRtc.WebRtcVadSharp;

public class WebRtcVadSharpDetectorFactory(WebRtcVadSharpOptions webRtcVadSharpOptions) : IVadDetectorFactory
{
    public IVadDetector CreateVadDetector(VadDetectorOptions options)
    {
        return new WebRtcVadSharpDetector(options, webRtcVadSharpOptions);
    }
}
