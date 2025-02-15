// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using EchoSharp.VoiceActivityDetection;

namespace EchoSharp.WebRtc.WebRtcVadSharp;

public class WebRtcVadSharpDetectorProvisioner(WebRtcVadSharpConfig config) : IVadDetectorProvisioner
{
    public async Task<IVadDetectorFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        var options = new WebRtcVadSharpOptions()
        {
            OperatingMode = config.OperatingMode
        };

        return await new WebRtcVadSharpDetectorFactory(options).WarmUpAsync(config.WarmUp, cancellationToken);
    }
}
