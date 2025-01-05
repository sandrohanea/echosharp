// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EchoSharp.Provisioning;
using EchoSharp.SpeechTranscription;
using SharpCompress.Archives;
using SharpCompress.Common;
using SherpaOnnx;

namespace EchoSharp.Onnx.Sherpa.SpeechTranscription;

public class SherpaOnnxSpeechTranscriptorProvisioner(SherpaOnnxSpeechTranscriptorConfig config) : ISpeechTranscriptorProvisioner
{
    private readonly HttpClient httpClient = new();

    public async Task<ISpeechTranscriptorFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        var path = config.BaseModelPath ?? Path.GetTempPath();
        var modelArchivePath = Path.Combine(path, $"{config.Model.Name}.tar.bz2");

        var downloaded = false;

        if (!File.Exists(modelArchivePath))
        {
            using var stream = await GetModelStreamAsync(cancellationToken);
            await stream.SaveToFileAsync(modelArchivePath, config.Model.Hash, cancellationToken);
            downloaded = true;
        }
        else if (config.CheckModelSize)
        {
            using var stream = await GetModelStreamAsync(cancellationToken);
            var modelSize = stream.Length;
            var fileSize = new FileInfo(modelArchivePath).Length;
            if (modelSize != fileSize)
            {
                using var newStream = await GetModelStreamAsync(cancellationToken);
                await newStream.SaveToFileAsync(modelArchivePath, config.Model.Hash, cancellationToken);
                downloaded = true;
            }
        }

        // if we downloaded the model, we need to check the hash as well
        if (downloaded)
        {
            var hash = await modelArchivePath.GetSha512HashAsync(cancellationToken);
            if (hash != config.Model.Hash)
            {
                throw new InvalidDataException("Downloaded model hash does not match the expected hash.");
            }
        }

        var modelPath = Path.Combine(path, config.Model.Name);
        if (!Directory.Exists(modelPath))
        {
            Directory.CreateDirectory(modelPath);
            // Open the .tar.bz2 archive
            using var archive = ArchiveFactory.Open(modelArchivePath);

            // Extract entries to the output directory
            foreach (var entry in archive.Entries)
            {
                if (!entry.IsDirectory && entry.Key != null)
                {
                    string destinationPath = Path.Combine(path, entry.Key);

                    // Create directory structure if necessary
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

                    // Extract the file
                    entry.WriteToFile(destinationPath, new ExtractionOptions { Overwrite = true });
                }
            }
        }
        SherpaOnnxSpeechTranscriptorFactory factory;

        if (config.Model is SherpaOnnxOnlineModel onlineModel)
        {
            var onlineOptions = new SherpaOnnxOnlineTranscriptorOptions();
            onlineOptions.Features = config.Features;
            var onlineModelConfig = new OnlineModelConfig();
            onlineModel.Load(modelPath, ref onlineModelConfig);
            onlineOptions.OnlineModelConfig = onlineModelConfig;
            factory = new(onlineOptions);
        }
        else if (config.Model is SherpaOnnxOfflineModel offlineModel)
        {
            var offlineOptions = new SherpaOnnxOfflineTranscriptorOptions();
            offlineOptions.Features = config.Features;
            var offlineModelConfig = new OfflineModelConfig();
            offlineModel.Load(modelPath, ref offlineModelConfig);
            offlineOptions.OfflineModelConfig = offlineModelConfig;
            factory = new(offlineOptions);
        }
        else
        {
            throw new NotSupportedException($"Model type {config.Model.GetType()} is not supported.");
        }

        return await factory.WarmUpAsync(config.WarmUp, cancellationToken);
    }

    private Task<Stream> GetModelStreamAsync(CancellationToken cancellationToken)
    {
        var url = new Uri($"https://github.com/k2-fsa/sherpa-onnx/releases/download/asr-models/{config.Model.Name}.tar.bz2");
#if NET8_0_OR_GREATER
        return httpClient.GetStreamAsync(url, cancellationToken);
#else
        return httpClient.GetStreamAsync(url);
#endif
    }
}
