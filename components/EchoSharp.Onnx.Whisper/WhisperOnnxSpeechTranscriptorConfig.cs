// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Onnx.Whisper;

public class WhisperOnnxSpeechTranscriptorConfig
{
    /// <summary>
    /// The path to the model file to be downloaded and used for transcription.
    /// </summary>
    /// <remarks>
    /// If this is empty, the model file will be downloaded to a temporary location and used for transcription.
    /// Defaults to <see langword="null"/>.
    /// </remarks>
    public string? ModelFileName { get; set; }

    /// <summary>
    /// The model type to be downloaded.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="WhisperOnnxModelType.Small"/>.
    /// </remarks>
    public WhisperOnnxModelType ModelType { get; set; } = WhisperOnnxModelType.Small;

    /// <summary>
    /// A flag to check if the model file size is the same as the downloaded model file size.
    /// </summary>
    /// <remarks>
    /// Only used if <see cref="ModelFileName"/> is set.
    /// Defaults to <see langword="false"/>.
    /// </remarks>
    public bool CheckModelSize { get; set; }

    /// <summary>
    /// A flag to run an initial warmup of the model.
    /// </summary>
    /// <remarks>
    /// For some runtimes, it is beneficial to run an initial warmup of the model as initial inference can be slower.
    /// By default, the model is warmed up.
    /// </remarks>
    public bool WarmUp { get; set; } = true;
}
