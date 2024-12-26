// Licensed under the MIT license: https://opensource.org/licenses/MIT

using WebRtcVadSharp;

namespace EchoSharp.WebRtc.WebRtcVadSharp;

public class WebRtcVadSharpOptions
{
    public OperatingMode OperatingMode { get; set; } = OperatingMode.HighQuality;
}
