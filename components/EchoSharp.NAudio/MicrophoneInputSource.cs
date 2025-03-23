// Licensed under the MIT license: https://opensource.org/licenses/MIT

using NAudio.Wave;
using EchoSharp.Audio;
using NAudio.CoreAudioApi;
using System.Runtime.InteropServices;
using EchoSharp.Audio.Source.Awaitable;

namespace EchoSharp.NAudio;

/// <summary>
/// Represents a source that captures audio from a microphone.
/// </summary>
public class MicrophoneInputSource : AwaitableWaveFileSource
{
    private IWaveIn? waveIn;

    public MicrophoneInputSource(int deviceNumber = 0,
                                 int sampleRate = 16000,
                                 int bitsPerSample = 16,
                                 int channels = 1,
                                 bool storeSamples = true,
                                 bool storeBytes = false,
                                 int initialSizeFloats = DefaultInitialSize,
                                 int initialSizeBytes = DefaultInitialSize,
                                 IChannelAggregationStrategy? aggregationStrategy = null)
        : base(storeSamples, storeBytes, initialSizeFloats, initialSizeBytes, aggregationStrategy)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException("EchoSharp.NAudio only supports Windows. For other platforms, please use a different audio component.");
        }

        var waveFormat = new WaveFormat(sampleRate, bitsPerSample, channels);

        waveIn = new WaveInEvent
        {
            DeviceNumber = deviceNumber,
            WaveFormat = waveFormat
        };

        Initialize(new AudioHeader()
        {
            BitsPerSample = (ushort)bitsPerSample,
            Channels = (ushort)channels,
            SampleRate = (uint)sampleRate
        });

        waveIn.DataAvailable += WaveIn_DataAvailable;
        waveIn.RecordingStopped += MicrophoneIn_RecordingStopped;
    }

    public void StartRecording()
    {
        waveIn?.StartRecording();
    }

    public void StopRecording()
    {
        waveIn?.StopRecording();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (waveIn != null)
            {
                waveIn.DataAvailable -= WaveIn_DataAvailable;
                waveIn.RecordingStopped -= MicrophoneIn_RecordingStopped;
                waveIn.Dispose();
                waveIn = null;
            }
        }
        base.Dispose(disposing);
    }

    private void MicrophoneIn_RecordingStopped(object? sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            throw e.Exception;
        }
        Flush();
    }

    private void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
    {
        WriteData(e.Buffer.AsMemory(0, e.BytesRecorded));
    }
}
