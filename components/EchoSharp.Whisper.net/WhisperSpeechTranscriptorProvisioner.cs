// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using EchoSharp.SpeechTranscription;
using Whisper.net;
using Whisper.net.Ggml;

namespace EchoSharp.Whisper.net;

public class WhisperSpeechTranscriptorProvisioner(WhisperSpeechTranscriptorConfig config) : ISpeechTranscriptorProvisioner
{
    private const int defaultBufferCopyLength = 81920;

    public async Task<ISpeechTranscriptorFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        if (config.OpenVinoEncoderModelPath is not null)
        {
            await WhisperGgmlDownloader.GetEncoderOpenVinoModelAsync(config.GgmlType, cancellationToken)
                                       .ExtractToPath(config.OpenVinoEncoderModelPath);
        }

        if (config.CoreMLEncoderModelPath is not null)
        {
            await WhisperGgmlDownloader.GetEncoderCoreMLModelAsync(config.GgmlType, cancellationToken)
                            .ExtractToPath(config.CoreMLEncoderModelPath);
        }

        if (config.ModelFilePath is null)
        {
            using var ggmlModelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(config.GgmlType, config.QuantizationType, cancellationToken);
            using var memoryStream = new MemoryStream();
            await ggmlModelStream.CopyToAsync(memoryStream, defaultBufferCopyLength, cancellationToken);
            // TODO: Use Memory directly instead of copying to a new buffer (once #316 will be released)
            return await new WhisperSpeechTranscriptorFactory(WhisperFactory.FromBuffer(memoryStream.ToArray(), config.WhisperFactoryOptions)).WarmUpAsync(config.WarmUp, cancellationToken);
        }

        if (!File.Exists(config.ModelFilePath))
        {
            using var ggmlModelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(config.GgmlType, config.QuantizationType, cancellationToken);

            await ggmlModelStream.SaveToFileAsync(config.ModelFilePath, cancellationToken);
        }
        else if (config.CheckModelSize)
        {
            using var ggmlModelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(config.GgmlType, config.QuantizationType, cancellationToken);
            var modelSize = ggmlModelStream.Length;
            var fileSize = new FileInfo(config.ModelFilePath).Length;
            if (modelSize != fileSize)
            {
                await ggmlModelStream.SaveToFileAsync(config.ModelFilePath, cancellationToken);
            }
        }

        return await new WhisperSpeechTranscriptorFactory(config.ModelFilePath).WarmUpAsync(config.WarmUp, cancellationToken);
    }

}
