// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using EchoSharp.VoiceActivityDetection;

namespace EchoSharp.Onnx.SileroVad;

public class SileroVadProvisioner(SileroVadConfig config) : IVadDetectorProvisioner
{
    private readonly HttpClient httpClient = new();
    public async Task<IVadDetectorFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        var modelPath = config.ModelFilePath ??
            Path.Combine(Path.GetTempPath(), "silero-vad-models", $"{config.ModelType}.onnx");

        if (!File.Exists(modelPath))
        {
            var modelStream = await GetModelStreamAsync(cancellationToken);
        }

        var options = new SileroVadOptions(modelPath)
        {
            Threshold = config.Threshold,
            ThresholdGap = config.ThresholdGap
        };

        return new SileroVadDetectorFactory(options);
    }

    private Task<Stream> GetModelStreamAsync(CancellationToken cancellationToken)
    {
        var url = config.ModelType switch
        {
            SileroVadModelType.Op15_16k => "https://github.com/sandrohanea/silero-vad/raw/refs/heads/master/src/silero_vad/data/silero_vad_16k_op15.onnx",
            SileroVadModelType.Half => "https://github.com/sandrohanea/silero-vad/raw/refs/heads/master/src/silero_vad/data/silero_vad_half.onnx",
            SileroVadModelType.Full => "https://github.com/sandrohanea/silero-vad/raw/refs/heads/master/src/silero_vad/data/silero_vad.onnx",
            _ => throw new ArgumentException("Invalid model type.")
        };

#if NET8_0_OR_GREATER
        return httpClient.GetStreamAsync(url, cancellationToken);
#else
        return httpClient.GetStreamAsync(url);
#endif
    }
}
