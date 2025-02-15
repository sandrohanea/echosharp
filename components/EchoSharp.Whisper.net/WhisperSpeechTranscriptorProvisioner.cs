// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Unarchive;
using EchoSharp.SpeechTranscription;
using Whisper.net;

namespace EchoSharp.Whisper.net;

public class WhisperSpeechTranscriptorProvisioner(
    WhisperSpeechTranscriptorConfig config,
    Func<WhisperProcessorBuilder, WhisperProcessorBuilder>? builderConfig = null,
    ModelDownloader? modelDownloader = null) : ISpeechTranscriptorProvisioner
{
    // All whisper.net models are less than 4GB
    private const long maxFileSize = 4L * 1024 * 1024 * 1024; // 4 GB

    public async Task<ISpeechTranscriptorFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        var currentModelDownloader = modelDownloader ?? ModelDownloader.Default;
        if (config.OpenVinoEncoderModelPath is not null)
        {
            var openVinoModel = WhisperNetModels.GetOpenVinoModel(config.GgmlType);
            await currentModelDownloader.DownloadModelAsync(
                openVinoModel,
                new UnarchiverOptions(config.OpenVinoEncoderModelPath, maxFileSize),
                Sha512Hasher.Instance,
                UnarchiverZip.Instance,
                cancellationToken);
        }

        if (config.CoreMLEncoderModelPath is not null)
        {
            var openVinoModel = WhisperNetModels.GetCoreMlModel(config.GgmlType);
            await currentModelDownloader.DownloadModelAsync(
                openVinoModel,
                new UnarchiverOptions(config.CoreMLEncoderModelPath, maxFileSize),
                Sha512Hasher.Instance,
                UnarchiverZip.Instance,
                cancellationToken);
        }

        if (config.ModelPath is not null)
        {
            var options = new UnarchiverOptions(config.ModelPath, maxFileSize);
            await currentModelDownloader.DownloadModelAsync(WhisperNetModels.GetGgmlModel(config.QuantizationType, config.GgmlType), options, Sha512Hasher.Instance, UnarchiverCopy.Instance, cancellationToken);
            var modelGgmlPath = Path.Combine(config.ModelPath, UnarchiverCopy.ModelName);
            return await new WhisperSpeechTranscriptorFactory(modelGgmlPath, config.WhisperFactoryOptions, builderConfig)
                .WarmUpAsync(config.WarmUp, cancellationToken);

        }
        using var memoryModel = new MemoryModel();

        await currentModelDownloader.DownloadModelAsync(
            WhisperNetModels.GetGgmlModel(config.QuantizationType, config.GgmlType),
            new UnarchiverOptions(memoryModel, maxFileSize),
            Sha512Hasher.Instance,
            UnarchiverCopy.Instance,
            cancellationToken);

        return await new WhisperSpeechTranscriptorFactory(memoryModel.GetModelMemory(UnarchiverCopy.ModelName), config.WhisperFactoryOptions, builderConfig)
            .WarmUpAsync(config.WarmUp, cancellationToken);
    }
}
