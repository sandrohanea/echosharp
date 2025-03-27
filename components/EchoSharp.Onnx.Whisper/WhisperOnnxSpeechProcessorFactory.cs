// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.SpeechProcessing;

namespace EchoSharp.Onnx.Whisper;

/// <summary>
/// Factory for creating instances of a processor that uses ONNX models to transcribe speech.
/// </summary>
/// <remarks>
/// For now, it has limited support with ONNX Models available here: https://huggingface.co/khmyznikov/whisper-int8-cpu-ort.onnx
/// </remarks>
public sealed class WhisperOnnxSpeechProcessorFactory : ISpeechProcessorFactory
{
    private readonly string? modelPath;
    private readonly byte[]? modelBytes;

    public WhisperOnnxSpeechProcessorFactory(string modelPath)
    {
        this.modelPath = modelPath;
    }

    public WhisperOnnxSpeechProcessorFactory(byte[] modelBytes)
    {
        this.modelBytes = modelBytes;
    }

    public ISpeechProcessor Create(SpeechProcessorOptions options)
    {
        if (options.Type != SpeechProcessingType.Transcript)
        {
            throw new NotSupportedException("Only Transcript processing is supported by WhisperOnnxSpeechProcessor");
        }

        return modelPath != null
            ? new WhisperOnnxSpeechProcessor(modelPath, options)
            : new WhisperOnnxSpeechProcessor(modelBytes!, options);
    }

    public void Dispose()
    {
    }
}
