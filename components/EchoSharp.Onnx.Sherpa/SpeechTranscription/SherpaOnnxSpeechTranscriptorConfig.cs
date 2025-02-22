// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Onnx.Sherpa.SpeechTranscription;

public class SherpaOnnxSpeechTranscriptorConfig
{
    public SherpaOnnxModel Model { get; set; } = SherpaOnnxModels.ZipFormerGigaSpeechInt8;

    /// <summary>
    /// The number of features to use for the model.
    /// </summary>
    /// <remarks>
    /// Default is 80.
    /// </remarks>
    public int Features { get; set; } = 80;

    /// <summary>
    /// The path to download the model.
    /// </summary>
    public string ModelPath { get; set; } = ".";

    /// <summary>
    /// A flag to run an initial warmup of the model.
    /// </summary>
    /// <remarks>
    /// For some runtimes, it is beneficial to run an initial warmup of the model as initial inference can be slower.
    /// By default, the model is warmed up.
    /// </remarks>
    public bool WarmUp { get; set; } = true;
}
