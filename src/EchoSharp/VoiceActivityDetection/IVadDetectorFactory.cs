// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.VoiceActivityDetection;

public interface IVadDetectorFactory : IDisposable
{
    /// <summary>
    /// Creates the vad detector using the specified options.
    /// </summary>
    IVadDetector CreateVadDetector(VadDetectorOptions options);
}
