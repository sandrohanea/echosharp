// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Abstractions.Audio;

namespace EchoSharp.Audio;

/// <summary>
/// Represents an audio source that is backed by samples loaded into memory either serialized as bytes or as floats.
/// </summary>
public readonly struct MemoryAudioSource : IAudioSource, IMemoryBackedAudioSource
{
    private readonly Memory<float>? floatFrames;
    private readonly Memory<byte>? byteFrames;
    private readonly uint sampleRate;
    private readonly ushort channelCount;
    private readonly ushort bitsPerSample;
    private readonly long framesCount;
    private readonly int frameSize;

    public MemoryAudioSource(Memory<float>? floatFrames, Memory<byte>? byteFrames, uint sampleRate, ushort channelCount = 1, ushort bitsPerSample = 16)
    {
        if (floatFrames == null && byteFrames == null)
        {
            throw new ArgumentNullException(nameof(floatFrames), "At least one backing memory needs to be not null");
        }

        if (floatFrames.HasValue && byteFrames.HasValue && floatFrames.Value.Length != (byteFrames.Value.Length * 8 / bitsPerSample))
        {
            // We need to check that the number of frames is the same in both cases
            throw new ArgumentException("The number of frames in the float and byte arrays do not match");
        }

        this.floatFrames = floatFrames;
        this.byteFrames = byteFrames;
        this.sampleRate = sampleRate;
        this.channelCount = channelCount;
        this.bitsPerSample = bitsPerSample;
        frameSize = channelCount * bitsPerSample / 8;

        if (floatFrames.HasValue)
        {
            framesCount = floatFrames.Value.Length / channelCount;
        }
        else
        {
            framesCount = byteFrames!.Value.Length / frameSize;
        }

        Duration = TimeSpan.FromSeconds((double)framesCount / sampleRate);
    }

    /// <inheritdoc />
    public long FramesCount => framesCount;

    /// <inheritdoc />
    public ushort ChannelCount => channelCount;

    /// <inheritdoc />
    public bool IsInitialized => true;

    /// <inheritdoc />
    public uint SampleRate => sampleRate;

    public ushort BitsPerSample => bitsPerSample;

    public bool StoresFloats => floatFrames.HasValue;

    public bool StoresBytes => byteFrames.HasValue;

    public TimeSpan Duration { get; }

    public TimeSpan TotalDuration => Duration;

    /// <inheritdoc />
    public void Dispose()
    {
        // No need to dispose this type of audio source (no resources to release).
    }

    public Task<Memory<byte>> GetFramesAsync(long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default)
    {
        if (byteFrames.HasValue)
        {
            return Task.FromResult(GetByteFramesSlice(startFrame, maxFrames));
        }

        var slice = GetFloatFramesSlice(startFrame, maxFrames);
        return Task.FromResult(SampleSerializer.Serialize(slice, BitsPerSample));
    }

    public Task<Memory<float>> GetSamplesAsync(long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default)
    {
        if (floatFrames.HasValue)
        {
            return Task.FromResult(GetFloatFramesSlice(startFrame, maxFrames));
        }

        var byteSlice = GetByteFramesSlice(startFrame, maxFrames);
        return Task.FromResult(SampleSerializer.Deserialize(byteSlice, BitsPerSample));
    }

    public Task<int> CopyFramesAsync(Memory<byte> destination, long startFrame, int maxFrames = int.MaxValue, CancellationToken cancellationToken = default)
    {
        if (byteFrames != null)
        {
            var slice = GetByteFramesSlice(startFrame, maxFrames);

            slice.CopyTo(destination);
            var byteFrameCount = slice.Length / frameSize;
            return Task.FromResult(byteFrameCount);
        }

        var floatSlice = GetFloatFramesSlice(startFrame, maxFrames);
        SampleSerializer.Serialize(floatSlice, destination, BitsPerSample);

        var frameCount = floatSlice.Length / ChannelCount;
        return Task.FromResult(frameCount);
    }

    private Memory<float> GetFloatFramesSlice(long startFrame, int maxFrames)
    {
        var startSample = (int)(startFrame * ChannelCount);
        var length = (int)(Math.Min(maxFrames, FramesCount - startFrame) * ChannelCount);
        return floatFrames!.Value.Slice(startSample, length);
    }

    private Memory<byte> GetByteFramesSlice(long startFrame, int maxFrames)
    {
        var startByte = (int)(startFrame * frameSize);
        var lengthBytes = (int)(Math.Min(maxFrames, FramesCount - startFrame) * frameSize);
        return byteFrames!.Value.Slice(startByte, lengthBytes);
    }

}
