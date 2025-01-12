// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.SpeechTranscription;

namespace EchoSharp.Onnx.Whisper;

/// <summary>
/// Factory for creating instances of a transcriptor that uses ONNX models to transcribe speech.
/// </summary>
/// <remarks>
/// For now, it has limited support with ONNX Models available here: https://huggingface.co/khmyznikov/whisper-int8-cpu-ort.onnx
/// </remarks>
public sealed class WhisperOnnxSpeechTranscriptorFactory : ISpeechTranscriptorFactory
{
    private readonly string? modelPath;
    private readonly byte[]? modelBytes;

    public WhisperOnnxSpeechTranscriptorFactory(string modelPath)
    {
        this.modelPath = modelPath;
    }

    public WhisperOnnxSpeechTranscriptorFactory(byte[] modelBytes)
    {
        this.modelBytes = modelBytes;
    }

    public ISpeechTranscriptor Create(SpeechTranscriptorOptions options)
    {
        return modelPath != null
            ? new WhisperOnnxSpeechTranscriptor(modelPath, options)
            : new WhisperOnnxSpeechTranscriptor(modelBytes!, options);
    }

    public void Dispose()
    {
    }
}
