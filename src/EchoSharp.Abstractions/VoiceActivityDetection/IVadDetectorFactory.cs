// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Abstractions.VoiceActivityDetection;

public interface IVadDetectorFactory
{
    /// <summary>
    /// Creates the vad detector using the specified options.
    /// </summary>
    IVadDetector CreateVadDetector(VadDetectorOptions options);
}
