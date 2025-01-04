// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using EchoSharp.Audio;
using EchoSharp.VoiceActivityDetection;
using WebRtcVadSharp;

namespace EchoSharp.WebRtc.WebRtcVadSharp;

internal sealed class WebRtcVadSharpDetector : IVadDetector, IDisposable
{
    private const int batchSize = 512;
    private readonly WebRtcVad vad;
    private readonly TimeSpan minSpeechDuration;
    private readonly TimeSpan minSilenceDuration;

    public WebRtcVadSharpDetector(VadDetectorOptions vadDetectorOptions, WebRtcVadSharpOptions? options = null)
    {
        vad = new WebRtcVad
        {
            OperatingMode = options?.OperatingMode ?? OperatingMode.HighQuality,
        };
        minSpeechDuration = vadDetectorOptions.MinSpeechDuration;
        minSilenceDuration = vadDetectorOptions.MinSilenceDuration;
    }

    public async IAsyncEnumerable<VadSegment> DetectSegmentsAsync(IAudioSource source, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (source.ChannelCount != 1)
        {
            throw new NotSupportedException("Only mono-channel audio is supported. Consider one channel aggregation on the audio source.");
        }

        var sampleRate = source.SampleRate switch
        {
            8000 => SampleRate.Is8kHz,
            16000 => SampleRate.Is16kHz,
            32000 => SampleRate.Is32kHz,
            48000 => SampleRate.Is48kHz,
            _ => throw new NotSupportedException("The sample rate is not supported.")
        };

        var bytesPerSecond = source.SampleRate * source.ChannelCount * 4;
        var frameLength = bytesPerSecond switch
        {
            32000 => FrameLength.Is20ms,
            48000 => FrameLength.Is30ms,
            _ => FrameLength.Is10ms
        };

        var minSpeechSamples = (int)(source.SampleRate / 1000 * minSpeechDuration.TotalMilliseconds);
        var minSilenceSamples = (int)(source.SampleRate / 1000 * minSilenceDuration.TotalMilliseconds);

        int? startingIndex = null;
        int? startingSilenceIndex = null;

        var samples = await source.GetSamplesAsync(0, cancellationToken: cancellationToken);
        // buffer for the 16bit linear PCM
        var batch = new byte[batchSize * 2];
        for (var i = 0; i < samples.Length - batchSize; i += batchSize)
        {
            var slice = samples.Slice(i, batchSize);

            // Serialize the samples to the 16bit linear PCM buffer

            var floatSpan = slice.Span;
            for (var indexSample = 0; indexSample < floatSpan.Length; indexSample++)
            {
                var sample = floatSpan[indexSample];
                //convert (-1, 1) range int to short
                var sixteenbit = (short)(sample * 32767);
                BinaryPrimitives.WriteInt16LittleEndian(batch.AsSpan(indexSample * 2, 2), sixteenbit);
            }

            var isSpeech = vad.HasSpeech(batch, sampleRate, frameLength);

            if (!startingIndex.HasValue)
            {
                if (isSpeech)
                {
                    startingIndex = i;
                }
                continue;
            }

            // We are in speech
            if (isSpeech)
            {
                startingSilenceIndex = null;
                continue;
            }

            if (startingSilenceIndex == null)
            {
                startingSilenceIndex = i;
                continue;
            }

            if (startingSilenceIndex == null)
            {
                // We are still in speech and the current batch is between the threshold and the negative threshold
                // We continue to the next batch
                continue;
            }

            var silenceLength = i - startingSilenceIndex.Value;

            if (silenceLength > minSilenceSamples)
            {
                // We have silence after speech exceeding the minimum silence duration
                var length = i - startingIndex.Value;
                if (length >= minSpeechSamples)
                {
                    yield return new VadSegment
                    {
                        StartTime = TimeSpan.FromMilliseconds(startingIndex.Value * 1000d / source.SampleRate),
                        Duration = TimeSpan.FromMilliseconds(length * 1000d / source.SampleRate)
                    };
                }
                startingIndex = null;
                startingSilenceIndex = null;
            }
        }

        // The last segment if it was already started (might be incomplete for sources that are not yet finished)
        if (startingIndex.HasValue)
        {
            var length = samples.Length - startingIndex.Value;
            if (length >= minSpeechSamples)
            {
                yield return new VadSegment
                {
                    StartTime = TimeSpan.FromMilliseconds(startingIndex.Value * 1000d / source.SampleRate),
                    Duration = TimeSpan.FromMilliseconds(length * 1000d / source.SampleRate),
                    IsIncomplete = true
                };
            }
        }
    }

    public void Dispose()
    {
        vad.Dispose();
    }
}
