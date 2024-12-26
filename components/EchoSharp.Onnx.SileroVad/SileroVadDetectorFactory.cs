// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Abstractions.VoiceActivityDetection;

namespace EchoSharp.Onnx.SileroVad;

/// <summary>
/// Silero VAD detector that uses the Silero VAD ONNX model to detect voice activity segments in mono-channel audio samples at 16 kHz.
/// </summary>
/// <remarks>
/// The ONNX model can be downloaded / exported from https://github.com/snakers4/silero-vad/blob/master/src/silero_vad/data/silero_vad.onnx
/// </remarks>
public class SileroVadDetectorFactory(SileroVadOptions sileroVadOptions) : IVadDetectorFactory
{
    public IVadDetector CreateVadDetector(VadDetectorOptions options)
    {
        return new SileroVadDetector(options, sileroVadOptions);
    }
}
