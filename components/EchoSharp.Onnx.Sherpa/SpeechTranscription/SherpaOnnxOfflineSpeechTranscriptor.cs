// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EchoSharp.Abstractions.Audio;
using EchoSharp.Abstractions.SpeechTranscription;
using EchoSharp.Audio;
using EchoSharp.Onnx.Sherpa.Internals;
using SherpaOnnx;

namespace EchoSharp.Onnx.Sherpa.SpeechTranscription;

internal sealed partial class SherpaOnnxOfflineSpeechTranscriptor(SherpaProvider<OfflineRecognizer> recognizers, SherpaProvider<SilenceAudioSource> silenceSources) : ISpeechTranscriptor
{
    // AcceptWaveform is not accepting Memory<float> nor Span<float> directly (only array) so we use the underlying native call directly in order to not copy the memory for each call.

#if NET8_0_OR_GREATER
    [LibraryImport("sherpa-onnx-c-api", EntryPoint = "SherpaOnnxAcceptWaveformOffline")]
    private static partial void AcceptWaveform(IntPtr handle, int sampleRate, IntPtr samples, int n);
#else
    [DllImport("sherpa-onnx-c-api", EntryPoint = "SherpaOnnxAcceptWaveformOffline")]
    private static extern void AcceptWaveform(IntPtr handle, int sampleRate, IntPtr samples, int n);
#endif

    public void Dispose()
    {
    }

    public async IAsyncEnumerable<TranscriptSegment> TranscribeAsync(IAudioSource source, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var recognizer = recognizers.Get((int)source.SampleRate);

        var samplesMemory = await source.GetSamplesAsync(0, cancellationToken: cancellationToken);

        using var stream = recognizer.CreateStream();

        AddWaveData(stream, samplesMemory);

        var silence = silenceSources.Get((int)source.SampleRate);

        var silenceSamples = await silence.GetSamplesAsync(0, cancellationToken: cancellationToken);
        AddWaveData(stream, silenceSamples);

        recognizer.Decode(stream);

        var text = stream.Result.Text;
        var tokens = stream.Result.Tokens;
        var timeStamps = stream.Result.Timestamps;
        var transcriptSegment = new TranscriptSegment()
        {
            Text = text
        };

        var samplesDuration = TimeSpan.FromSeconds(samplesMemory.Length / (double)source.SampleRate);
        if (tokens != null || timeStamps != null)
        {
            var tokensLength = (tokens != null && timeStamps != null)
                ? Math.Min(tokens.Length, timeStamps.Length)
                : tokens != null
                        ? tokens.Length
                        : timeStamps!.Length;
            var tokensList = new List<TranscriptToken>(tokensLength);
            var startTime = TimeSpan.Zero;

            for (var i = 1; i < tokensLength; i++)
            {
                tokensList.Add(new TranscriptToken()
                {
                    Text = tokens?[i - 1],
                    StartTime = timeStamps != null ? TimeSpan.FromSeconds(timeStamps[i - 1]) : TimeSpan.Zero,
                    Duration = timeStamps != null ? TimeSpan.FromSeconds(timeStamps[i]) - TimeSpan.FromSeconds(timeStamps[i - 1]) : TimeSpan.Zero
                });
            }

            if (tokensLength != 0)
            {
                startTime = timeStamps != null ? TimeSpan.FromSeconds(timeStamps[0]) : TimeSpan.Zero;

                var lastStartTime = timeStamps != null ? TimeSpan.FromSeconds(timeStamps[tokensLength - 1]) : TimeSpan.Zero;
                tokensList.Add(new TranscriptToken()
                {
                    Text = tokens?[tokensLength - 1],
                    StartTime = lastStartTime,
                    Duration = samplesDuration - lastStartTime
                });
            }

            transcriptSegment.Tokens = tokensList;
            transcriptSegment.Duration = samplesDuration - startTime;
            transcriptSegment.StartTime = startTime;
        }
        else
        {
            transcriptSegment.Duration = samplesDuration;
            transcriptSegment.StartTime = TimeSpan.Zero;
        }

        yield return transcriptSegment;
    }

    private static unsafe void AddWaveData(OfflineStream stream, Memory<float> samples)
    {
        fixed (float* samplesPtr = samples.Span)
        {
            AcceptWaveform(stream.Handle, 16000, (IntPtr)samplesPtr, samples.Length);
        }
    }
}
