// Licensed under the MIT license: https://opensource.org/licenses/MIT

using WebRtcVadSharp;

namespace EchoSharp.WebRtc.WebRtcVadSharp;

public class WebRtcVadSharpConfig
{
    /// <summary>
    /// Gets or sets the operating mode.
    /// </summary>
    /// <remarks>
    /// By default, the operating mode is set to <see cref="OperatingMode.HighQuality"/>.
    /// </remarks>
    public OperatingMode OperatingMode { get; set; } = OperatingMode.HighQuality;

    /// <summary>
    /// A flag to run an initial warmup of the model.
    /// </summary>
    /// <remarks>
    /// For some runtimes, it is beneficial to run an initial warmup of the model as initial inference can be slower.
    /// By default, the model is warmed up.
    /// </remarks>
    public bool WarmUp { get; set; } = true;
}
