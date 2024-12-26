// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Onnx.SileroVad;

internal class SileroConstants
{
    public const int BatchSize = 512;
    public const int ContextSize = 64;
    public const int StateSize = 256;
    public const int OutputSize = 1;
    public const int SampleRate = 16000;
}
