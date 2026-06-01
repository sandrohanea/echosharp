// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.CompilerServices;
using EchoSharp.Audio;
using EchoSharp.VoiceActivityDetection;
using Whisper.net;

namespace EchoSharp.Whisper.net.SileroVad;

internal sealed class WhisperSileroVadDetector : IVadDetector
{
    private readonly WhisperVadFactory vadFactory;
    private readonly WhisperVadProcessor vadProcessor;

    public WhisperSileroVadDetector(
        VadDetectorOptions vadDetectorOptions,
        WhisperSileroVadOptions whisperSileroVadOptions,
        Func<WhisperVadProcessorBuilder, WhisperVadProcessorBuilder>? builderConfig = null)
    {
        vadFactory = WhisperVadFactory.FromPath(whisperSileroVadOptions.ModelPath, whisperSileroVadOptions.WhisperFactoryOptions);

        var builder = vadFactory.CreateBuilder()
            .WithThreshold(whisperSileroVadOptions.Threshold)
            .WithMinSpeechDuration(vadDetectorOptions.MinSpeechDuration)
            .WithMinSilenceDuration(vadDetectorOptions.MinSilenceDuration);

        if (whisperSileroVadOptions.MaxSpeechDuration.HasValue)
        {
            builder = builder.WithMaxSpeechDuration(whisperSileroVadOptions.MaxSpeechDuration.Value);
        }

        if (whisperSileroVadOptions.SpeechPadding.HasValue)
        {
            builder = builder.WithSpeechPadding(whisperSileroVadOptions.SpeechPadding.Value);
        }

        if (whisperSileroVadOptions.SamplesOverlap.HasValue)
        {
            builder = builder.WithSamplesOverlap(whisperSileroVadOptions.SamplesOverlap.Value);
        }

        if (whisperSileroVadOptions.Threads.HasValue)
        {
            builder = builder.WithThreads(whisperSileroVadOptions.Threads.Value);
        }

        if (whisperSileroVadOptions.UseGpu.HasValue)
        {
            builder = builder.WithUseGpu(whisperSileroVadOptions.UseGpu.Value);
        }

        if (whisperSileroVadOptions.GpuDevice.HasValue)
        {
            builder = builder.WithGpuDevice(whisperSileroVadOptions.GpuDevice.Value);
        }

        if (builderConfig is not null)
        {
            builder = builderConfig(builder);
        }

        vadProcessor = builder.Build();
    }

    public async IAsyncEnumerable<VadSegment> DetectSegmentsAsync(IAudioSource source, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        ValidateSource(source);

        var samples = await source.GetSamplesAsync(0, cancellationToken: cancellationToken);
        var segments = await vadProcessor.DetectSpeechAsync((ReadOnlyMemory<float>)samples, cancellationToken);

        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            cancellationToken.ThrowIfCancellationRequested();
            var start = segment.Start < TimeSpan.Zero ? TimeSpan.Zero : segment.Start;
            var end = segment.End > source.Duration ? source.Duration : segment.End;
            if (end <= start)
            {
                continue;
            }

            yield return new VadSegment
            {
                StartTime = start,
                Duration = end - start,
                IsIncomplete = i == segments.Count - 1 && end >= source.Duration - TimeSpan.FromMilliseconds(10)
            };
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
            throw new NotSupportedException("Only 16 kHz audio is supported. Consider resampling before calling this detector.");
        }
    }

    public void Dispose()
    {
        vadProcessor.Dispose();
        vadFactory.Dispose();
    }
}
