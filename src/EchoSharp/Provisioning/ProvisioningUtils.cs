// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Audio;
using EchoSharp.Audio.Sink;
using EchoSharp.SpeechSynthesis;
using EchoSharp.SpeechTranscription;
using EchoSharp.VoiceActivityDetection;

namespace EchoSharp.Provisioning;

public static class ProvisioningUtils
{
    public static async Task<ISpeechTranscriptorFactory> WarmUpAsync(this ISpeechTranscriptorFactory factory, bool warmUp, CancellationToken cancellationToken)
    {
        if (warmUp)
        {
            var options = new SpeechTranscriptorOptions()
            {
                LanguageAutoDetect = false,
                RetrieveTokenDetails = true,
            };
            using var warmUpTranscritor = factory.Create(options);
            using var silenceAudio = new SilenceAudioSource(TimeSpan.FromMilliseconds(1111), 16000);
            var transcribeEvents = warmUpTranscritor.TranscribeAsync(silenceAudio, cancellationToken);
            await foreach (var _ in transcribeEvents.WithCancellation(cancellationToken))
            {
                // Do nothing
            }
        }
        return factory;
    }

    public static async Task<IVadDetectorFactory> WarmUpAsync(this IVadDetectorFactory factory, bool warmUp, CancellationToken cancellationToken)
    {
        if (warmUp)
        {
            var options = new VadDetectorOptions();
            using var warmUpDetector = factory.CreateVadDetector(options);
            using var silenceAudio = new SilenceAudioSource(TimeSpan.FromMilliseconds(1111), 16000);
            var vadEvents = warmUpDetector.DetectSegmentsAsync(silenceAudio, cancellationToken);
            await foreach (var _ in vadEvents.WithCancellation(cancellationToken))
            {
                // Do nothing
            }
        }
        return factory;
    }

    public static async Task<IRealtimeSpeechTranscriptorFactory> WarmUpAsync(this IRealtimeSpeechTranscriptorFactory factory, bool warmUp, CancellationToken cancellationToken)
    {
        if (warmUp)
        {
            var options = new RealtimeSpeechTranscriptorOptions()
            {
                LanguageAutoDetect = false,
            };
            using var warmUpTranscritor = factory.Create(options);
            using var completedSilence = new CompletedAudioSource(new SilenceAudioSource(TimeSpan.FromSeconds(1), 16000));
            var transcribeEvents = warmUpTranscritor.TranscribeAsync(completedSilence, cancellationToken);
            await foreach (var _ in transcribeEvents.WithCancellation(cancellationToken))
            {
                // Do nothing
            }
        }
        return factory;
    }

    public static async Task<ISpeechSynthesizerFactory> WarmUpAsync(this ISpeechSynthesizerFactory factory, bool warmUp, CancellationToken cancellationToken)
    {
        if (warmUp)
        {
            var options = new SpeechSynthesizerOptions();
            using var warmUpSynthesizer = factory.Create(options);
            await using var nullAudioSink = new NullAudioSink();

            await warmUpSynthesizer.SynthesizeAsync(new SpeechSegment()
            {
                Text = "Hello, this is a warm-up test.",
            }, nullAudioSink, cancellationToken);
        }
        return factory;
    }
}
