// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Unarchive;
using EchoSharp.VoiceActivityDetection;

namespace EchoSharp.Onnx.SileroVad;

public class SileroVadProvisioner(SileroVadConfig config, ModelDownloader? modelDownloader = null) : IVadDetectorProvisioner
{
    private const int maxModelSize = 2327524;

    public async Task<IVadDetectorFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        var currentModelDownloader = modelDownloader ?? ModelDownloader.Default;
        if (config.ModelPath is not null)
        {
            var options = new UnarchiverOptions(config.ModelPath, maxModelSize);
            await currentModelDownloader.DownloadModelAsync(config.Model, options, Sha512Hasher.Instance, UnarchiverCopy.Instance, cancellationToken);
            var modelOnnxPath = Path.Combine(config.ModelPath, UnarchiverCopy.ModelName);

            return await new SileroVadDetectorFactory(new SileroVadOptions(modelOnnxPath)
            {
                Threshold = config.Threshold,
                ThresholdGap = config.ThresholdGap
            }).WarmUpAsync(config.WarmUp, cancellationToken);

        }
        using var memoryModel = new MemoryModel();

        await currentModelDownloader.DownloadModelAsync(
            config.Model,
            new UnarchiverOptions(memoryModel, maxModelSize),
            Sha512Hasher.Instance,
            UnarchiverCopy.Instance,
            cancellationToken);

        return await new SileroVadDetectorFactory(new SileroVadOptions(memoryModel.GetModelMemory(UnarchiverCopy.ModelName).ToArray())
        {
            Threshold = config.Threshold,
            ThresholdGap = config.ThresholdGap
        }).WarmUpAsync(config.WarmUp, cancellationToken);
    }

}
