// Licensed under the MIT license: https://opensource.org/licenses/MIT

using NAudio.Wave;
using EchoSharp.Audio;

namespace EchoSharp.NAudio;

/// <summary>
/// Represents a source that captures audio from a microphone.
/// </summary>
public class MicrophoneInputSource : AwaitableWaveFileSource
{
    private readonly WaveInEvent microphoneIn;

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
        microphoneIn = new WaveInEvent
        {
            DeviceNumber = deviceNumber,
            WaveFormat = new WaveFormat(sampleRate, bitsPerSample, channels)
        };

        Initialize(new AudioSourceHeader()
        {
            BitsPerSample = (ushort)bitsPerSample,
            Channels = (ushort)channels,
            SampleRate = (uint)sampleRate
        });

        microphoneIn.DataAvailable += WaveIn_DataAvailable;
        microphoneIn.RecordingStopped += MicrophoneIn_RecordingStopped;
    }

    public void StartRecording()
    {
        microphoneIn.StartRecording();
    }

    public void StopRecording()
    {
        microphoneIn.StopRecording();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            microphoneIn.DataAvailable -= WaveIn_DataAvailable;
            microphoneIn.RecordingStopped -= MicrophoneIn_RecordingStopped;
            microphoneIn.Dispose();
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
