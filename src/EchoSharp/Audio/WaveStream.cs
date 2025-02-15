// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Buffers;
using System.Buffers.Binary;
using EchoSharp.Config;

namespace EchoSharp.Audio;

/// <summary>
/// Represents a stream of audio samples in the WAVE format backed by an audio source.
/// </summary>
/// <param name=""></param>
public class WaveFileStream : Stream
{
    private readonly byte[] headerBuffer;
    private readonly IAudioSource source;

    public WaveFileStream(IAudioSource source)
    {
        this.source = source;
        headerBuffer = BuildHeader();
    }

    private byte[] BuildHeader()
    {
        var header = ArrayPool<byte>.Shared.Rent(44);

        // RIFF Chunk Descriptor
        header[0] = (byte)'R'; // ChunkID
        header[1] = (byte)'I';
        header[2] = (byte)'F';
        header[3] = (byte)'F';

        var chunkSize = (uint)(36 + source.ChannelCount * source.FramesCount * source.BitsPerSample / 8);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(4, 4), chunkSize);

        header[8] = (byte)'W'; // Format
        header[9] = (byte)'A';
        header[10] = (byte)'V';
        header[11] = (byte)'E';

        // fmt Subchunk
        header[12] = (byte)'f'; // Subchunk1ID
        header[13] = (byte)'m';
        header[14] = (byte)'t';
        header[15] = (byte)' ';

        header[16] = 16; // Subchunk1Size (PCM = 16)
        header[20] = 1;  // AudioFormat (PCM = 1)

        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(22), source.ChannelCount); // NumChannels
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(24), source.SampleRate); // SampleRate

        var byteRate = source.SampleRate * source.ChannelCount * source.BitsPerSample / 8;
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(28), byteRate); // ByteRate

        var blockAlign = (ushort)(source.ChannelCount * source.BitsPerSample / 8);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(32), blockAlign); // BlockAlign

        header[34] = (byte)source.BitsPerSample; // BitsPerSample

        // data Subchunk
        header[36] = (byte)'d'; // Subchunk2ID
        header[37] = (byte)'a';
        header[38] = (byte)'t';
        header[39] = (byte)'a';

        var subchunk2Size = (uint)(source.ChannelCount * source.FramesCount * source.BitsPerSample / 8);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(40), subchunk2Size); // Subchunk2Size

        return header;
    }

    public override bool CanRead => true;

    public override bool CanSeek => true;

    public override bool CanWrite => false;

    public override long Length => 44 + source.ChannelCount * source.FramesCount * source.BitsPerSample / 8;

    public override long Position { get; set; }

#if NET8_0_OR_GREATER
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        return ReadInternalAsync(buffer, cancellationToken);
    }

    public override int Read(Span<byte> buffer)
    {
        return ReadInternal(buffer);
    }
#endif

    public async Task WriteToFileAsync(string fileName, CancellationToken cancellationToken = default)
    {
        using var fileStream = File.OpenWrite(fileName);

#if NET8_0_OR_GREATER
        await fileStream.WriteAsync(headerBuffer.AsMemory(0, 44), cancellationToken);
#else
        await fileStream.WriteAsync(headerBuffer, 0, 44, cancellationToken);
#endif
        var framesPerTurn = 2048;
        var bytesPerFrame = source.ChannelCount * source.BitsPerSample / 8;
        var buffer = ArrayPool<byte>.Shared.Rent(framesPerTurn * bytesPerFrame);
        try
        {
            var position = 0;
            while (true)
            {
                var actualRead = await source.CopyFramesAsync(buffer, position, framesPerTurn, cancellationToken);
                position += actualRead;
                var bytesRead = actualRead * bytesPerFrame;

#if NET8_0_OR_GREATER
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
#else
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
#endif
                if (framesPerTurn != actualRead)
                {
                    break;
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return ReadInternalAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return ReadInternal(buffer.AsSpan(offset, count));
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        var newPosition = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => Length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };
        if (newPosition < 0 || newPosition > Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }
        Position = newPosition;
        return Position;
    }

    public override void Flush()
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    protected override void Dispose(bool disposing)
    {
        ArrayPool<byte>.Shared.Return(headerBuffer, ArrayPoolConfig.ClearOnReturn);
        base.Dispose(disposing);
    }

    private async ValueTask<int> ReadInternalAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var totalBytesRead = 0;

        var remainingBytes = Length - Position;
        if (remainingBytes <= 0)
        {
            return 0;
        }

        var count = (int)Math.Min(buffer.Length, remainingBytes);
        var bufferOffset = 0;

        // Read from header if we are still within the header's bounds
        if (Position < 44)
        {
            var headerBytesRemaining = (int)(44 - Position);
            var bytesToCopy = Math.Min(count, headerBytesRemaining);
            headerBuffer.AsMemory((int)Position, bytesToCopy).CopyTo(buffer.Slice(bufferOffset, bytesToCopy));

            Position += bytesToCopy;
            totalBytesRead += bytesToCopy;
            bufferOffset += bytesToCopy;
            count -= bytesToCopy;

        }

        if (count == 0)
        {
            return totalBytesRead;
        }

        // We are done with the header, so we can just copy the audio data from the source
        var byteIndex = Position - 44;

        var bytesPerFrame = source.ChannelCount * source.BitsPerSample / 8;
        var startFrame = byteIndex / bytesPerFrame;
        var frameByteOffset = (int)(byteIndex % bytesPerFrame);

        // Calculate how many frames we need to read
        var framesToRead = (int)Math.Ceiling((double)(count + frameByteOffset) / bytesPerFrame);

        // Make sure we don't read beyond the source frames
        var totalFramesAvailable = source.FramesCount - startFrame;
        if (totalFramesAvailable <= 0)
        {
            return totalBytesRead;
        }

        framesToRead = (int)Math.Min(framesToRead, totalFramesAvailable);

        // Get frames from the source
        var actualFramesRead = await source.CopyFramesAsync(buffer.Slice(bufferOffset), startFrame, framesToRead, cancellationToken);
        var bytesCopiedFromFrames = actualFramesRead * bytesPerFrame;

        Position += bytesCopiedFromFrames;
        totalBytesRead += bytesCopiedFromFrames;

        return totalBytesRead;
    }

    private int ReadInternal(Span<byte> buffer)
    {
        throw new NotImplementedException();
    }
}
