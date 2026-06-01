// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net;
using Whisper.net.Ggml;

namespace EchoSharp.Whisper.net.SileroVad;

public class WhisperSileroVadConfig
{
    /// <summary>
    /// The directory where the ggml Silero VAD model will be downloaded and used.
    /// </summary>
    /// <remarks>
    /// Whisper.net 1.9.1 loads ggml Silero VAD models from a file path, so this value is required.
    /// </remarks>
    public string? ModelPath { get; set; }

    /// <summary>
    /// The ggml Silero VAD model version to use.
    /// </summary>
    public SileroVadType ModelType { get; set; } = SileroVadType.V6_2_0;

    /// <summary>
    /// The probability threshold to consider audio as speech.
    /// </summary>
    public float Threshold { get; set; } = 0.5f;

    /// <summary>
    /// The maximum duration of a speech segment before Whisper.net forces a new segment.
    /// </summary>
    public TimeSpan? MaxSpeechDuration { get; set; }

    /// <summary>
    /// The padding added before and after speech segments.
    /// </summary>
    public TimeSpan? SpeechPadding { get; set; }

    /// <summary>
    /// The overlap used when copying audio samples from speech segments.
    /// </summary>
    public TimeSpan? SamplesOverlap { get; set; }

    /// <summary>
    /// The number of threads used by the Whisper.net VAD processor.
    /// </summary>
    public int? Threads { get; set; }

    /// <summary>
    /// Enables or disables GPU usage for Whisper.net VAD.
    /// </summary>
    public bool? UseGpu { get; set; }

    /// <summary>
    /// The GPU device used by Whisper.net VAD.
    /// </summary>
    public int? GpuDevice { get; set; }

    /// <summary>
    /// The options used when loading the Whisper.net VAD model.
    /// </summary>
    public WhisperFactoryOptions WhisperFactoryOptions { get; set; } = new();

    /// <summary>
    /// A flag to run an initial warmup of the model.
    /// </summary>
    public bool WarmUp { get; set; } = true;
}
