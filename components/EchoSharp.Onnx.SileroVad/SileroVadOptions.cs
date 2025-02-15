// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Onnx.SileroVad;
public class SileroVadOptions
{
    public SileroVadOptions(string modelPath)
    {
        ModelPath = modelPath;
        ModelBytes = [];
    }

    public SileroVadOptions(byte[] modelBytes)
    {
        ModelBytes = modelBytes;
    }

    public string? ModelPath { get; set; }

    public float ThresholdGap { get; set; } = 0.15f;

    public float Threshold { get; set; } = 0.5f;
    public byte[] ModelBytes { get; set; }
}
