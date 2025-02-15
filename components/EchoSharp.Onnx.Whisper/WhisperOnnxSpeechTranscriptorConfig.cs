// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Onnx.Whisper;

public class WhisperOnnxSpeechTranscriptorConfig
{
    /// <summary>
    /// The path where the model file will be downloaded and used for transcription.
    /// </summary>
    /// <remarks>
    /// If this is set, the model file will be downloaded and used for transcription.
    /// If this is not set, the model will be downloaded in memory and used for transcription (not persisted to disk).
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public string? ModelPath { get; set; }

    /// <summary>
    /// The model type to be downloaded.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="WhisperOnnxModelType.Small"/>.
    /// </remarks>
    public WhisperOnnxModelType ModelType { get; set; } = WhisperOnnxModelType.Small;

    /// <summary>
    /// A flag to run an initial warmup of the model.
    /// </summary>
    /// <remarks>
    /// For some runtimes, it is beneficial to run an initial warmup of the model as initial inference can be slower.
    /// By default, the model is warmed up.
    /// </remarks>
    public bool WarmUp { get; set; } = true;
}
