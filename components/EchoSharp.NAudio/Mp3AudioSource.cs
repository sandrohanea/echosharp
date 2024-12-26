// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Audio;
using NAudio.Wave;

namespace EchoSharp.NAudio;

public class Mp3AudioSource(Mp3FileReader mp3FileReader, bool leaveOpen = false, int bufferLength = PcmStreamSource.DefaultBufferLength, IChannelAggregationStrategy? aggregationStrategy = null)
    : PcmStreamSource(mp3FileReader, leaveOpen, bufferLength, aggregationStrategy)
{
    public Mp3AudioSource(string fileName, int bufferLength = DefaultBufferLength, IChannelAggregationStrategy? aggregationStrategy = null)
        : this(new Mp3FileReader(fileName), leaveOpen: false, bufferLength, aggregationStrategy)
    {
    }

    public override TimeSpan Duration => mp3FileReader.TotalTime;

    public override TimeSpan TotalDuration => mp3FileReader.TotalTime;

    public override uint SampleRate => (uint)mp3FileReader.WaveFormat.SampleRate;

    public override long FramesCount => mp3FileReader.Length / mp3FileReader.BlockAlign;

    public override bool IsInitialized => true;

    public override ushort BitsPerSample => (ushort)mp3FileReader.WaveFormat.BitsPerSample;

    // The data chunk offset is always 0 for MP3 files
    protected override int DataChunkOffset => 0;
    protected override ushort SourceChannelCount => (ushort)mp3FileReader.WaveFormat.Channels;

    public override Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        // The Mp3FileReader is already initialized
        return Task.CompletedTask;
    }
}
