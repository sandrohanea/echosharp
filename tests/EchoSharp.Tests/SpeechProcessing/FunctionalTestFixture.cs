// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Onnx.Sherpa.SpeechTranscription;
using EchoSharp.Onnx.SileroVad;
using EchoSharp.SpeechProcessing;
using EchoSharp.VoiceActivityDetection;
using EchoSharp.WebRtc.WebRtcVadSharp;
using EchoSharp.Whisper.net;
using Whisper.net.Ggml;
using Xunit;

namespace EchoSharp.Tests.SpeechProcessing;

public class FunctionalTestFixture : IDisposable
{
    private IVadDetectorFactory? sileroVadDetectorFactory;
    private IVadDetectorFactory? webRtcVadDetectorFactory;
    private ISpeechProcessorFactory? whisperProcessorFactory;
    private ISpeechProcessorFactory? sherpaProcessorFactory;
    private readonly SemaphoreSlim semaphoreSlim = new(1, 1);

    public Task<ISpeechProcessorFactory> GetWhisperProcessorFactoryAsync()
    {
        return ProvisionAndCacheAsync(
            async () => whisperProcessorFactory = await new WhisperSpeechProcessorProvisioner(new WhisperSpeechProcessorConfig()
            {
                GgmlType = GgmlType.Small
            }).ProvisionAsync(),
            () => whisperProcessorFactory);
    }

    public Task<ISpeechProcessorFactory> GetSherpaProcessorFactoryAsync()
    {
        return ProvisionAndCacheAsync(
            async () => sherpaProcessorFactory =
                await new SherpaOnnxSpeechProcessorProvisioner(new SherpaOnnxSpeechProcessorConfig() { }).ProvisionAsync(),
            () => sherpaProcessorFactory);
    }

    public Task<IVadDetectorFactory> GetWebRtcVadDetectorFactoryAsync()
    {
        return ProvisionAndCacheAsync(
            async () => webRtcVadDetectorFactory =
                await new WebRtcVadSharpDetectorProvisioner(new WebRtcVadSharpConfig()).ProvisionAsync(),
            () => webRtcVadDetectorFactory
        );
    }

    public Task<IVadDetectorFactory> GetSileroVadDetectorFactoryAsync()
    {
        return ProvisionAndCacheAsync(
            async () => sileroVadDetectorFactory = await new SileroVadProvisioner(new SileroVadConfig()).ProvisionAsync(),
            () => sileroVadDetectorFactory);
    }

    public void Dispose()
    {
        sileroVadDetectorFactory?.Dispose();
        webRtcVadDetectorFactory?.Dispose();
        whisperProcessorFactory?.Dispose();
        sherpaProcessorFactory?.Dispose();
    }

    private async Task<T> ProvisionAndCacheAsync<T>(Func<Task> provisioner, Func<T?> cacheGet)
    {
        var cached = cacheGet();
        if (cached != null)
        {
            return cached;
        }
        await semaphoreSlim.WaitAsync();
        try
        {
            cached = cacheGet();
            if (cached != null)
            {
                return cached;
            }
            await provisioner();
            return cacheGet()!;

        }
        finally
        {
            semaphoreSlim.Release();
        }
    }
}
