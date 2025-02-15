// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Audio;
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
            var transcribeEvents = warmUpTranscritor.TranscribeAsync(new SilenceAudioSource(TimeSpan.FromMilliseconds(1111), 16000), cancellationToken);
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
            var vadEvents = warmUpDetector.DetectSegmentsAsync(new SilenceAudioSource(TimeSpan.FromSeconds(1), 16000), cancellationToken);
            await foreach (var _ in vadEvents.WithCancellation(cancellationToken))
            {
                // Do nothing
            }
        }
        return factory;
    }
}
