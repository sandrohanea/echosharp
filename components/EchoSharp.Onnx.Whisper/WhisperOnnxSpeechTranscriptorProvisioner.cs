// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Unarchive;
using EchoSharp.SpeechTranscription;

namespace EchoSharp.Onnx.Whisper;

public class WhisperOnnxSpeechTranscriptorProvisioner(WhisperOnnxSpeechTranscriptorConfig config) : ISpeechTranscriptorProvisioner
{
    private const int maxModelSize = 1366071277;

    private static readonly ProvisioningModel tinyModel = new(
        new("https://huggingface.co/khmyznikov/whisper-int8-cpu-ort.onnx/resolve/main/whisper_tiny_int8_cpu_ort_1.18.0.onnx"),
        ProvisioningModel.ArchiveTypes.None,
        "67tAKlQULgTnjPjX0bsYLmrduJ8cMP6b7313KoDX5agzmQgyv+nUakRT+fSaX03XpSW3Wt2K6V6QvGQg1ofCbw==",
        "21ATap8Dl1j1z+FYPP8VvW37zXU85jmpf1QhDe6AWpPvJVKTEeFr2WOAFQ2IAjvKtbk4W8nLZu6v94U8mF7J8g==",
        77356551,
        77356551);
    private static readonly ProvisioningModel smallModel = new(
        new("https://huggingface.co/khmyznikov/whisper-int8-cpu-ort.onnx/resolve/main/whisper_small_int8_cpu_ort_1.18.0.onnx"),
        ProvisioningModel.ArchiveTypes.None,
        "OMnrccFdFx7nCZMmd7YC74UpXjG4uTFb8gCwe2prcp1+89vC/axdXFAA73y4Ii77aoUzcEV6pUoIq4f1vue2CA==",
        "fMJ10hDbqT+zo+LuiyhIeaTFc3QRdxm2co73+XpsCWRHqyUt1nwxUhzEf0dB8XWZ1UhoM3iq5h0ft8AGWAPUmA==",
        443051369,
        443051369);
    private static readonly ProvisioningModel mediumModel = new(
        new("https://huggingface.co/khmyznikov/whisper-int8-cpu-ort.onnx/resolve/main/whisper_medium_int8_cpu_ort_1.18.0.onnx"),
        ProvisioningModel.ArchiveTypes.None,
        "vhTdXQu6JNTeVY57nJf8n3dEY4ZMea6Dr/47R3yr1PI6+z2YB5lMCdJhxG2v9oWqGdcb63kKMZAZxGz7uUfEWQ==",
        "lfBuUqC/Mv8VWBF8J8rkfkrcdr0qwVe6aN0seBGMHxqia5yvOedSoAhz0UF9fGKhQ88VphdFTZNmLo70bt02Iw==",
        1366071277,
        1366071277);

    public async Task<ISpeechTranscriptorFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        var provisioningModel = config.ModelType switch
        {
            WhisperOnnxModelType.Tiny => tinyModel,
            WhisperOnnxModelType.Small => smallModel,
            WhisperOnnxModelType.Medium => mediumModel,
            _ => throw new NotSupportedException($"Model type {config.ModelType} is not supported.")
        };

        if (config.ModelPath != null)
        {
            await ModelDownloader.DownloadModelAsync(provisioningModel,
                new UnarchiverOptions(config.ModelPath, maxModelSize),
                Sha512Hasher.Instance,
                UnarchiverCopy.Instance,
                cancellationToken);

            var modelOnnxPath = Path.Combine(config.ModelPath, UnarchiverCopy.ModelName);
            return await new WhisperOnnxSpeechTranscriptorFactory(modelOnnxPath).WarmUpAsync(config.WarmUp, cancellationToken);
        }

        using var memoryModel = new MemoryModel();

        await ModelDownloader.DownloadModelAsync(provisioningModel,
            new UnarchiverOptions(memoryModel, maxModelSize),
            Sha512Hasher.Instance,
            UnarchiverCopy.Instance,
            cancellationToken);

        return await new WhisperOnnxSpeechTranscriptorFactory(memoryModel.GetModelMemory(UnarchiverCopy.ModelName).ToArray())
            .WarmUpAsync(config.WarmUp, cancellationToken);
    }
}
