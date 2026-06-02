// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.VoiceActivityDetection;
using Whisper.net;

namespace EchoSharp.Whisper.net.SileroVad;

public class WhisperSileroVadDetectorFactory(
    WhisperSileroVadOptions whisperSileroVadOptions,
    Func<WhisperVadProcessorBuilder, WhisperVadProcessorBuilder>? builderConfig = null) : IVadDetectorFactory
{
    public IVadDetector CreateVadDetector(VadDetectorOptions options)
    {
        return new WhisperSileroVadDetector(options, whisperSileroVadOptions, builderConfig);
    }
}
