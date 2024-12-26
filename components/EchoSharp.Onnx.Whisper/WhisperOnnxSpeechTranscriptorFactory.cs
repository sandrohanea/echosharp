// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Abstractions.SpeechTranscription;

namespace EchoSharp.Onnx.Whisper;

/// <summary>
/// Factory for creating instances of a transcriptor that uses ONNX models to transcribe speech.
/// </summary>
/// <remarks>
/// For now, it has limited support with ONNX Models available here: https://huggingface.co/khmyznikov/whisper-int8-cpu-ort.onnx
/// </remarks>
public sealed class WhisperOnnxSpeechTranscriptorFactory(string modelPath) : ISpeechTranscriptorFactory
{
    public ISpeechTranscriptor Create(SpeechTranscriptorOptions options)
    {
        return new WhisperOnnxSpeechTranscriptor(modelPath, options);
    }

    public void Dispose()
    {
    }
}
