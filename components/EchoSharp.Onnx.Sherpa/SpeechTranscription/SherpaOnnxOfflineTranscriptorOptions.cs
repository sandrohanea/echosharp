// Licensed under the MIT license: https://opensource.org/licenses/MIT

using SherpaOnnx;

namespace EchoSharp.Onnx.Sherpa.SpeechTranscription;

public class SherpaOnnxOfflineTranscriptorOptions
{
    /// <summary>
    /// Configuration for the offline model.
    /// </summary>
    public OfflineModelConfig OfflineModelConfig { get; set; } = new();

    /// <summary>
    /// The number of features to be used with the Sherpa models.
    /// </summary>
    public int Features { get; set; } = 80;
}
