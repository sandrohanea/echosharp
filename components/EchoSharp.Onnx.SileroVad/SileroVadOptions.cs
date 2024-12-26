// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Onnx.SileroVad;
public class SileroVadOptions(string modelPath)
{
    public string ModelPath { get; set; } = modelPath;

    public float ThresholdGap { get; set; } = 0.15f;

    public float Threshold { get; set; } = 0.5f;
}
