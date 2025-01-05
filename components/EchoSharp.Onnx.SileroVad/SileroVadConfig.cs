// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Onnx.SileroVad;

public class SileroVadConfig
{
    /// <summary>
    /// The path to the model file to be downloaded and used for transcription.
    /// </summary>
    /// <remarks>
    /// If this is set, the model file will be downloaded and used for transcription.
    /// If this is not set, the model will be downloaded in memory and used for transcription (not persisted to disk).
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public string? ModelFilePath { get; set; }

    /// <summary>
    /// The model type to be used.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="SileroVadModelType.Full"/>.
    /// </remarks>
    public SileroVadModelType ModelType { get; set; } = SileroVadModelType.Full;

    /// <summary>
    /// The gap threshold to be used when detecting voice activity.
    /// </summary>
    /// <remarks>
    /// This gap is subtracted from the threshold to detect when the voice activity goes from active to inactive.
    /// Default value is 0.15.
    /// Example: If the threshold is 0.5 and the gap threshold is 0.15, the voice activity will be moved from active to inactive when the voice activity confidence goes below 0.35.
    /// </remarks>
    public float ThresholdGap { get; set; } = 0.15f;

    /// <summary>
    /// The threshold to be used when detecting voice activity.
    /// </summary>
    /// <remarks>
    /// The gap threshold is used to detect when the voice activity goes from active to inactive.
    /// Default value is 0.5.
    /// </remarks>
    public float Threshold { get; set; } = 0.5f;

    /// <summary>
    /// A flag to run an initial warmup of the model.
    /// </summary>
    /// <remarks>
    /// For some runtimes, it is beneficial to run an initial warmup of the model as initial inference can be slower.
    /// By default, the model is warmed up.
    /// </remarks>
    public bool WarmUp { get; set; } = true;
}
