// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Unarchive;
using EchoSharp.VoiceActivityDetection;
using Whisper.net;

namespace EchoSharp.Whisper.net.SileroVad;

public class WhisperSileroVadProvisioner(
    WhisperSileroVadConfig config,
    Func<WhisperVadProcessorBuilder, WhisperVadProcessorBuilder>? builderConfig = null,
    ModelDownloader? modelDownloader = null) : IVadDetectorProvisioner
{
    private const int maxModelSize = 885098;

    public async Task<IVadDetectorFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        var modelPath = config.ModelPath;
        if (modelPath is null || string.IsNullOrWhiteSpace(modelPath))
        {
            throw new InvalidOperationException("Whisper.net Silero VAD requires a ModelPath directory because Whisper.net 1.9.1 loads ggml VAD models from a file path.");
        }

        var currentModelDownloader = modelDownloader ?? ModelDownloader.Default;
        var model = WhisperSileroVadModels.GetModel(config.ModelType);
        var options = new UnarchiverOptions(modelPath, maxModelSize);
        await currentModelDownloader.DownloadModelAsync(model, options, Sha512Hasher.Instance, UnarchiverCopy.Instance, cancellationToken);

        var modelGgmlPath = Path.Combine(modelPath, UnarchiverCopy.ModelName);
        return await new WhisperSileroVadDetectorFactory(new WhisperSileroVadOptions(modelGgmlPath, config.WhisperFactoryOptions)
        {
            Threshold = config.Threshold,
            MaxSpeechDuration = config.MaxSpeechDuration,
            SpeechPadding = config.SpeechPadding,
            SamplesOverlap = config.SamplesOverlap,
            Threads = config.Threads,
            UseGpu = config.UseGpu,
            GpuDevice = config.GpuDevice
        }, builderConfig).WarmUpAsync(config.WarmUp, cancellationToken);
    }
}
