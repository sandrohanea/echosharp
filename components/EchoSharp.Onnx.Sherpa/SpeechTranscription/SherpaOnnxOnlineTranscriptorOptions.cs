// Licensed under the MIT license: https://opensource.org/licenses/MIT

using SherpaOnnx;

namespace EchoSharp.Onnx.Sherpa.SpeechTranscription;

public class SherpaOnnxOnlineTranscriptorOptions
{
    /// <summary>
    /// Configuration for the online model.
    /// </summary>
    public OnlineModelConfig OnlineModelConfig { get; set; } = new();

    /// <summary>
    /// Configuration for the online CTC FST decoder model.
    /// </summary>

    public OnlineCtcFstDecoderConfig OnlineCtcFstDecoderConfig { get; set; } = new();

    /// <summary>
    /// The number of features to be used with the Sherpa models.
    /// </summary>
    public int Features { get; set; } = 80;
}
