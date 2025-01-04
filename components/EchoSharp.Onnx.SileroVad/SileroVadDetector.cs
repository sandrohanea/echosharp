// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Buffers;
using System.Runtime.CompilerServices;
using EchoSharp.Audio;
using EchoSharp.Config;
using EchoSharp.VoiceActivityDetection;

namespace EchoSharp.Onnx.SileroVad;

internal class SileroVadDetector : IVadDetector
{
    private const float threshHoldGap = 0.15f;

    private readonly SileroVadOnnxModel model;
    private readonly float threshold;
    private readonly float negThreshold;
    private readonly int minSpeechSamples;
    private readonly int minSilenceSamples;

    public SileroVadDetector(VadDetectorOptions vadDetectorOptions, SileroVadOptions sileroVadOptions)
    {
        model = new(sileroVadOptions.ModelPath);
        threshold = sileroVadOptions?.Threshold ?? 0.5f;
        negThreshold = threshold - threshHoldGap;
        minSpeechSamples = (int)(SileroConstants.SampleRate / 1000 * vadDetectorOptions.MinSpeechDuration.TotalMilliseconds);
        minSilenceSamples = (int)(SileroConstants.SampleRate / 1000 * vadDetectorOptions.MinSilenceDuration.TotalMilliseconds);
    }

    public async IAsyncEnumerable<VadSegment> DetectSegmentsAsync(IAudioSource source, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ValidateSource(source);

        foreach (var segment in DetectSegments(await source.GetSamplesAsync(0, cancellationToken: cancellationToken)))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return segment;
        }
    }

    private IEnumerable<VadSegment> DetectSegments(Memory<float> samples)
    {
        var state = model.CreateInferenceState();
        int? startingIndex = null;
        int? startingSilenceIndex = null;

        for (var i = 0; i < samples.Length - SileroConstants.BatchSize; i += SileroConstants.BatchSize)
        {
            Memory<float> slice;
            float[]? rentedMemory = null;
            try
            {
                if (i == 0)
                {
                    // the first batch will have the empty context (which we need to copy at the beggining of the slice)
                    rentedMemory = ArrayPool<float>.Shared.Rent(SileroConstants.ContextSize + SileroConstants.BatchSize);
                    Array.Clear(rentedMemory, 0, SileroConstants.ContextSize);
                    samples.Span.Slice(0, SileroConstants.BatchSize).CopyTo(rentedMemory.AsSpan(SileroConstants.ContextSize));
                    slice = rentedMemory.AsMemory(0, SileroConstants.BatchSize + SileroConstants.ContextSize);
                }
                else
                {
                    slice = samples.Slice(i - SileroConstants.ContextSize, SileroConstants.BatchSize + SileroConstants.ContextSize);
                }

                var prob = model.Call(slice, state);
                if (!startingIndex.HasValue)
                {
                    if (prob > threshold)
                    {
                        startingIndex = i;
                    }
                    continue;
                }

                // We are in speech
                if (prob > threshold)
                {
                    startingSilenceIndex = null;
                    continue;
                }

                if (prob > negThreshold)
                {
                    // We are still in speech and the current batch is between the threshold and the negative threshold
                    // We continue to the next batch
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
                            StartTime = TimeSpan.FromMilliseconds(startingIndex.Value * 1000d / SileroConstants.SampleRate),
                            Duration = TimeSpan.FromMilliseconds(length * 1000d / SileroConstants.SampleRate)
                        };
                    }
                    startingIndex = null;
                    startingSilenceIndex = null;
                }
            }
            finally
            {
                if (rentedMemory != null)
                {
                    ArrayPool<float>.Shared.Return(rentedMemory, ArrayPoolConfig.ClearOnReturn);
                }
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
                    StartTime = TimeSpan.FromMilliseconds(startingIndex.Value * 1000d / SileroConstants.SampleRate),
                    Duration = TimeSpan.FromMilliseconds(length * 1000d / SileroConstants.SampleRate),
                    IsIncomplete = true
                };
            }
        }
    }

    private static void ValidateSource(IAudioSource source)
    {
        if (source.ChannelCount != 1)
        {
            throw new NotSupportedException("Only mono-channel audio is supported. Consider one channel aggregation on the audio source.");
        }

        if (source.SampleRate != 16000)
        {
            throw new NotSupportedException("Only 16 kHz audio is supported. Consider resampling before calling this transcriptor.");
        }
    }
}
