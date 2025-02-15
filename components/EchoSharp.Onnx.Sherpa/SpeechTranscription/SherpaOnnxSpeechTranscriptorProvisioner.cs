// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Unarchive;
using EchoSharp.SharpZipLib;
using EchoSharp.SpeechTranscription;

namespace EchoSharp.Onnx.Sherpa.SpeechTranscription;

public class SherpaOnnxSpeechTranscriptorProvisioner(SherpaOnnxSpeechTranscriptorConfig config, ModelDownloader? modelDownloader = null) : ISpeechTranscriptorProvisioner
{
    private const long maxModelSize = 4L * 1024 * 1024 * 1024;

    public async Task<ISpeechTranscriptorFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        var unarchiverOptions = new UnarchiverOptions(config.ModelPath, maxModelSize);
        var currentModelDownloader = modelDownloader ?? ModelDownloader.Default;

        await currentModelDownloader.DownloadModelAsync(config.Model, unarchiverOptions, Sha512Hasher.Instance, SharpZipLibUnarchiver.TarBz2, cancellationToken);

        var modelOnnxPath = Path.Combine(config.ModelPath, config.Model.Name);

        if (config.Model is SherpaOnnxOfflineModel offlineModel)
        {
            var offlineConfig = new SherpaOnnx.OfflineModelConfig();
            offlineModel.Load(modelOnnxPath, ref offlineConfig);
            var options = new SherpaOnnxOfflineTranscriptorOptions()
            {
                Features = config.Features,
                OfflineModelConfig = offlineConfig
            };
            return await new SherpaOnnxSpeechTranscriptorFactory(options).WarmUpAsync(config.WarmUp, cancellationToken);
        }
        if (config.Model is SherpaOnnxOnlineModel onlineModel)
        {
            var onlineConfig = new SherpaOnnx.OnlineModelConfig();
            onlineModel.Load(modelOnnxPath, ref onlineConfig);
            var options = new SherpaOnnxOnlineTranscriptorOptions()
            {
                Features = config.Features,
                OnlineModelConfig = onlineConfig
            };
            return await new SherpaOnnxSpeechTranscriptorFactory(options).WarmUpAsync(config.WarmUp, cancellationToken);
        }

        throw new NotSupportedException("Unsupported model type");
    }

}
