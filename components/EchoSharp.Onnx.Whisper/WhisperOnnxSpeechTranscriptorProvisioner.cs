// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using EchoSharp.SpeechTranscription;

namespace EchoSharp.Onnx.Whisper;

public class WhisperOnnxSpeechTranscriptorProvisioner(WhisperOnnxSpeechTranscriptorConfig config) : ISpeechTranscriptorProvisioner
{
    private readonly HttpClient httpClient = new();

    public async Task<ISpeechTranscriptorFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        var modelPath = config.ModelFileName is not null
            ? config.ModelFileName
            : GetTempModelFileName();

        if (!File.Exists(modelPath))
        {
            using var stream = await GetModelStreamAsync(cancellationToken);
            await stream.SaveToFileAsync(modelPath, cancellationToken);
        }
        else if (config.CheckModelSize)
        {
            using var stream = await GetModelStreamAsync(cancellationToken);
            var modelSize = stream.Length;
            var fileSize = new FileInfo(modelPath).Length;
            if (modelSize != fileSize)
            {
                using var newStream = await GetModelStreamAsync(cancellationToken);
                await newStream.SaveToFileAsync(modelPath, cancellationToken);
            }
        }

        return await new WhisperOnnxSpeechTranscriptorFactory(modelPath).WarmUpAsync(config.WarmUp, cancellationToken);
    }

    private string GetTempModelFileName()
    {
#if NET8_0_OR_GREATER
        var fileName = Enum.GetName(config.ModelType);
#else
        var fileName = Enum.GetName(typeof(WhisperOnnxModelType), config.ModelType);
#endif
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = "unknown";
        }

        var tempDirectory = Path.Combine(Path.GetTempPath(), "WhisperOnnx");
        if (!Directory.Exists(tempDirectory))
        {
            Directory.CreateDirectory(tempDirectory);
        }
        return Path.Combine(tempDirectory, $"whisper-{fileName}.onnx");
    }

    private Task<Stream> GetModelStreamAsync(CancellationToken cancellationToken)
    {
        var url = config.ModelType switch
        {
            WhisperOnnxModelType.Tiny => "https://huggingface.co/khmyznikov/whisper-int8-cpu-ort.onnx/resolve/main/whisper_tiny_int8_cpu_ort_1.18.0.onnx",
            WhisperOnnxModelType.Small => "https://huggingface.co/khmyznikov/whisper-int8-cpu-ort.onnx/resolve/main/whisper_small_int8_cpu_ort_1.18.0.onnx",
            WhisperOnnxModelType.Medium => "https://huggingface.co/khmyznikov/whisper-int8-cpu-ort.onnx/resolve/main/whisper_medium_int8_cpu_ort_1.18.0.onnx",
            _ => throw new NotSupportedException($"Model type {config.ModelType} is not supported.")
        };
#if NET8_0_OR_GREATER
        return httpClient.GetStreamAsync(url, cancellationToken);
#else
        return httpClient.GetStreamAsync(url);
#endif
    }
}
