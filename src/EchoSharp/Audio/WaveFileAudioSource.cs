// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Internals;

namespace EchoSharp.Audio;

/// <summary>
/// Represents an audio source that reads audio samples from a WAVE file (or stream representing a WAVE file).
/// </summary>
public class WaveFileAudioSource(Stream waveStream, bool leaveOpen = true, int bufferLength = PcmStreamSource.DefaultBufferLength, IChannelAggregationStrategy? aggregationStrategy = null)
    : PcmStreamSource(waveStream, leaveOpen, bufferLength, aggregationStrategy)
{
    private AudioSourceHeader? header;
    private int dataChunkOffset;
    private long dataChunkSize;
    private bool haveDataSize;

    public WaveFileAudioSource(string waveFile, int bufferLength = DefaultBufferLength, IChannelAggregationStrategy? aggregationStrategy = null)
        : this(File.OpenRead(waveFile), false, bufferLength, aggregationStrategy)
    {
    }

    public override TimeSpan Duration => TimeSpan.FromSeconds(FramesCount / (double)SampleRate);
    public override TimeSpan TotalDuration => Duration;

    public override long FramesCount => header != null
        ? (haveDataSize ? dataChunkSize : (PcmStream.Length - dataChunkOffset)) / SourceFrameSize
        : throw NotInitializedException();

    public override uint SampleRate => header?.SampleRate ?? throw NotInitializedException();
    public override bool IsInitialized => header != null;
    public override ushort BitsPerSample => header?.BitsPerSample ?? throw NotInitializedException();
    protected override ushort SourceChannelCount => header?.Channels ?? throw NotInitializedException();

    protected override int DataChunkOffset => dataChunkOffset;

    public override async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (IsInitialized)
        {
            return;
        }

        var headerResult = await WaveFileUtils.ParseHeaderAsync(PcmStream, cancellationToken);
        if (!headerResult.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to parse the wave header. Error: {headerResult.ErrorMessage}");
        }

        dataChunkOffset = headerResult.DataOffset;
        dataChunkSize = headerResult.DataChunkSize;
        header = headerResult.Header;
        haveDataSize = dataChunkSize > 0;
    }

    private static InvalidOperationException NotInitializedException()
    {
        return new InvalidOperationException("The source was not initialized");
    }

}
