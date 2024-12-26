// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Microsoft.ML.OnnxRuntime;

namespace EchoSharp.Onnx.SileroVad;

internal class SileroInferenceState(OrtIoBinding binding)
{
    /// Array for storing the context + input

    public float[] State { get; set; } = new float[SileroConstants.StateSize];

    // The state for the next inference
    public float[] PendingState { get; set; } = new float[SileroConstants.StateSize];

    public float[] Output { get; set; } = new float[SileroConstants.OutputSize];
    public OrtIoBinding Binding { get; } = binding;
}
