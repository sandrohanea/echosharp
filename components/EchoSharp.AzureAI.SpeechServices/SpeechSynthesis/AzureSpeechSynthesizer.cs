// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Audio.Sink;
using EchoSharp.AzureAI.SpeechServices.Internals;
using EchoSharp.SpeechSynthesis;
using EchoSharp.Internals;
using EchoSharp.Config;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Buffers;
using System.Globalization;
using EchoSharp.Audio;

namespace EchoSharp.AzureAI.SpeechServices.SpeechSynthesis;

internal sealed class AzureSpeechSynthesizer(
    SpeechConfig speechConfig,
    SpeechSynthesizerOptions options,
    AzureSpeechSynthesizerOptions azureOptions,
    AuthTokenHandler? authTokenHandler) : ISpeechSynthesizer
{
    public void Dispose()
    {
    }

    public async Task SynthesizeAsync(SpeechSegment speechSegment, IAudioSink audioSink,
        CancellationToken cancellationToken = default)
    {
        var audioOutputStream = new AudioSinkPushAudioOutputStream(audioSink);
        using var pushAudioOutputStream = new PushAudioOutputStream(audioOutputStream);
        using var audioConfig = AudioConfig.FromStreamOutput(pushAudioOutputStream);
        using var speechSynthesizer = new SpeechSynthesizer(speechConfig, audioConfig);

        // Configure voice from options
        if (!string.IsNullOrEmpty(options.DefaultVoice))
        {
            speechSynthesizer.Properties.SetProperty(PropertyId.SpeechServiceConnection_SynthVoice, options.DefaultVoice);
        }
        if (options.DefaultLanguage != null)
        {
            speechSynthesizer.Properties.SetProperty(PropertyId.SpeechServiceConnection_SynthLanguage, options.DefaultLanguage.Name);
        }

        // Configure Azure-specific options
        if (azureOptions.SpeakingRate != 1.0)
        {
            speechSynthesizer.Properties.SetProperty(PropertyId.SpeechSynthesisRequest_Rate, azureOptions.SpeakingRate.ToString("F2", CultureInfo.InvariantCulture));
        }
        if (azureOptions.Pitch != 1.0)
        {
            speechSynthesizer.Properties.SetProperty(PropertyId.SpeechSynthesisRequest_Pitch, azureOptions.Pitch.ToString("F2", CultureInfo.InvariantCulture));
        }
        if (azureOptions.Volume != 100)
        {
            speechSynthesizer.Properties.SetProperty(PropertyId.SpeechSynthesisRequest_Volume, azureOptions.Volume.ToString(CultureInfo.InvariantCulture));
        }

        await (authTokenHandler?.InitializeAsync(cancellationToken) ?? Task.CompletedTask);
        using var tokenLoader = new OptionalDisposable(authTokenHandler?.GetLoader(speechSynthesizer.Properties));
        var result = await speechSynthesizer.SpeakTextAsync(speechSegment.Text);
        
        // Wait for all writes to complete
        await audioOutputStream.CompleteAsync();
    }

    private class AudioSinkPushAudioOutputStream(IAudioSink audioSink) : PushAudioOutputStreamCallback
    {
        private readonly SemaphoreSlim writeLock = new(1, 1);
        private readonly Queue<(byte[] Buffer, int Length)> writeQueue = new();
        private readonly AsyncAutoResetEvent writeEvent = new();
        private Task? currentWriteTask;
        private bool isInitialized;
        private bool isCompleted;

        private async Task InitializeSinkAsync()
        {
            if (!isInitialized)
            {
                // Azure Speech SDK default format
                var header = new AudioHeader
                {
                    Channels = 1,  // mono
                    SampleRate = 16000,  // 16 kHz
                    BitsPerSample = 16   // 16-bit PCM
                };
                await audioSink.Initialize(header, duration: null);
                isInitialized = true;
            }
        }

        public override uint Write(byte[] dataBuffer)
        {
            if (isCompleted)
            {
                return 0;
            }

            // Rent a buffer from the pool and copy the data
            var rentedBuffer = ArrayPool<byte>.Shared.Rent(dataBuffer.Length);
            Array.Copy(dataBuffer, rentedBuffer, dataBuffer.Length);

            // Add to queue and signal the processing task
            lock (writeQueue)
            {
                writeQueue.Enqueue((rentedBuffer, dataBuffer.Length));
            }
            writeEvent.Set();

            // Start processing if not already running
            if (currentWriteTask == null || currentWriteTask.IsCompleted)
            {
                currentWriteTask = ProcessWriteQueueAsync();
            }

            return (uint)dataBuffer.Length;
        }

        public async Task CompleteAsync()
        {
            isCompleted = true;
            if (currentWriteTask != null)
            {
                writeEvent.Set();
                await currentWriteTask;
            }
        }

        private async Task ProcessWriteQueueAsync()
        {
            while (!isCompleted || writeQueue.Count > 0)
            {
                if (!isCompleted)
                {
                    await writeEvent.WaitAsync();
                }

                (byte[] Buffer, int Length)? bufferToWrite = null;
                lock (writeQueue)
                {
                    if (writeQueue.Count > 0)
                    {
                        bufferToWrite = writeQueue.Dequeue();
                    }
                }

                if (!bufferToWrite.HasValue)
                {
                    continue;
                }

                await writeLock.WaitAsync();
                try
                {
                    // Initialize on first write
                    await InitializeSinkAsync();
                    
                    // Write only the actual data length
                    await audioSink.WriteAsync(bufferToWrite.Value.Buffer.AsMemory(0, bufferToWrite.Value.Length));
                }
                finally
                {
                    writeLock.Release();
                    // Return the buffer to the pool
                    ArrayPool<byte>.Shared.Return(bufferToWrite.Value.Buffer, ArrayPoolConfig.ClearOnReturn);
                }
            }
        }
    }
}
