// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net;

namespace EchoSharp.Whisper.net.SileroVad;

public class WhisperSileroVadOptions(string modelPath, WhisperFactoryOptions? whisperFactoryOptions = null)
{
    public string ModelPath { get; } = modelPath;

    public WhisperFactoryOptions WhisperFactoryOptions { get; } = whisperFactoryOptions ?? new();

    public float Threshold { get; set; } = 0.5f;

    public TimeSpan? MaxSpeechDuration { get; set; }

    public TimeSpan? SpeechPadding { get; set; }

    public TimeSpan? SamplesOverlap { get; set; }

    public int? Threads { get; set; }

    public bool? UseGpu { get; set; }

    public int? GpuDevice { get; set; }
}
