// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Audio;
using EchoSharp.Onnx.Sherpa.Internals;
using EchoSharp.SpeechTranscription;
using SherpaOnnx;

namespace EchoSharp.Onnx.Sherpa.SpeechTranscription;

public sealed class SherpaOnnxSpeechTranscriptorFactory : ISpeechTranscriptorFactory
{
    private readonly SherpaProvider<SilenceAudioSource> silenceProvider;
    private readonly SherpaProvider<OfflineRecognizer>? offlineProvider;
    private readonly SherpaProvider<OnlineRecognizer>? onlineProvider;

    public SherpaOnnxSpeechTranscriptorFactory(SherpaOnnxOfflineTranscriptorOptions options)
    {
        // We need these providers as sample rate is required when recognizer is created, but we'll have it only when audio will be transcribed (later).
        silenceProvider = new((sr) => new SilenceAudioSource(TimeSpan.FromSeconds(0.3), (uint)sr));
        offlineProvider = new(sr =>
        {
            var offlineConfig = new OfflineRecognizerConfig
            {
                ModelConfig = options.OfflineModelConfig
            };
            offlineConfig.FeatConfig.FeatureDim = options.Features;
            offlineConfig.FeatConfig.SampleRate = sr;
            return new OfflineRecognizer(offlineConfig);
        });
    }

    public SherpaOnnxSpeechTranscriptorFactory(SherpaOnnxOnlineTranscriptorOptions options)
    {
        // We need these providers as sample rate is required when recognizer is created, but we'll have it only when audio will be transcribed (later).
        silenceProvider = new SherpaProvider<SilenceAudioSource>((sampleRate) => new SilenceAudioSource(TimeSpan.FromSeconds(0.3), (uint)sampleRate));
        onlineProvider = new(sr =>
        {
            var onlineConfig = new OnlineRecognizerConfig
            {
                ModelConfig = options.OnlineModelConfig
            };
            onlineConfig.FeatConfig.FeatureDim = options.Features;
            onlineConfig.FeatConfig.SampleRate = sr;
            onlineConfig.CtcFstDecoderConfig = options.OnlineCtcFstDecoderConfig;
            return new OnlineRecognizer(onlineConfig);
        });
    }

    public ISpeechTranscriptor Create(SpeechTranscriptorOptions options)
    {
        if (offlineProvider != null)
        {
            return new SherpaOnnxOfflineSpeechTranscriptor(offlineProvider, silenceProvider);
        }

        return new SherpaOnnxOnlineSpeechTranscriptor(onlineProvider!, silenceProvider);
    }

    public void Dispose()
    {
        silenceProvider?.Dispose();
        offlineProvider?.Dispose();
        onlineProvider?.Dispose();
    }
}
