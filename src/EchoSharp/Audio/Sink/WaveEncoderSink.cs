// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Buffers;
using System.Buffers.Binary;
using EchoSharp.Config;
using EchoSharp.Utils;

namespace EchoSharp.Audio.Sink;

/// <summary>
/// Base class for audio sinks that handles sample-to-WAV conversion.
/// </summary>
public abstract class WaveEncoderSink : IAudioSink
{
    private bool isInitialized;
    private bool isDisposed;
    private long totalFramesWritten;
    private double? duration;
    private AudioHeader audioHeader;
    protected readonly byte[] headerBuffer;

    protected WaveEncoderSink()
    {
        headerBuffer = ArrayPool<byte>.Shared.Rent(44);
    }

    /// <summary>
    /// Initializes the audio sink with the audio header and the duration of the audio stream.
    /// </summary>
    /// <remarks>
    /// The duration is optional and can be null.
    /// </remarks>
    public async Task Initialize(AudioHeader audioHeader, double? duration)
    {
        if (isInitialized)
        {
            throw new InvalidOperationException("The sink is already initialized.");
        }

        this.audioHeader = audioHeader;
        this.duration = duration;
        isInitialized = true;

        // Build and write the WAV header
        BuildHeader();
        await WriteAsync(headerBuffer.AsMemory(0, 44));
    }

    /// <summary>
    /// Writes audio data to the sink (append only).
    /// </summary>
    /// <remarks>
    /// The samples are written as a sequence of 32-bit floating-point values in the range [-1.0, 1.0].
    /// The number of samples must be a multiple of the number of channels.
    /// In order to use this method, the stream must be initialized first.
    /// </remarks>
    public async Task WriteAsync(ReadOnlyMemory<float> samples, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ThrowIfNotInitialized();

        if (samples.Length % audioHeader.Channels != 0)
        {
            throw new ArgumentException("The number of samples must be a multiple of the number of channels.", nameof(samples));
        }

        var bytes = SampleSerializer.Serialize(samples, audioHeader.BitsPerSample);
        await WriteAsync(bytes, cancellationToken);
        totalFramesWritten += samples.Length / audioHeader.Channels;
    }

    /// <summary>
    /// Writes raw audio data to the sink (append only) (wav format)
    /// </summary>
    /// <remarks>
    /// The data is written as a sequence of bytes.
    /// The bytes should have wav header appended first.
    /// </remarks>
    public abstract Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Throws if the object has been disposed.
    /// </summary>
    protected void ThrowIfDisposed()
    {
        if (isDisposed)
        {
            throw new ObjectDisposedException(nameof(WaveEncoderSink));
        }
    }

    /// <summary>
    /// Throws if the object is not initialized.
    /// </summary>
    protected void ThrowIfNotInitialized()
    {
        if (!isInitialized)
        {
            throw new InvalidOperationException("The sink is not initialized.");
        }
    }

    /// <summary>
    /// Disposes the audio sink asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (isDisposed)
        {
            return;
        }

        await DisposeAsyncCore();
        ArrayPool<byte>.Shared.Return(headerBuffer, ArrayPoolConfig.ClearOnReturn);
        isDisposed = true;
    }

    /// <summary>
    /// When overridden in a derived class, releases the unmanaged resources used by the audio sink.
    /// </summary>
    protected virtual ValueTask DisposeAsyncCore()
    {
        return EchoValueTask.Completed;
    }

    private void BuildHeader()
    {
        // RIFF Chunk Descriptor
        headerBuffer[0] = (byte)'R'; // ChunkID
        headerBuffer[1] = (byte)'I';
        headerBuffer[2] = (byte)'F';
        headerBuffer[3] = (byte)'F';

        // Calculate initial sizes based on duration if available
        uint dataSize = 0;
        var chunkSize = 0xFFFFFFFF; // Use -1 (0xFFFFFFFF) to indicate unknown/streaming size
        if (duration.HasValue && duration.Value > 0)
        {
            var bytesPerSecond = audioHeader.SampleRate * audioHeader.Channels * (audioHeader.BitsPerSample / 8.0);
            dataSize = (uint)(duration.Value * bytesPerSecond);
            chunkSize = 36 + dataSize;
        }

        BinaryPrimitives.WriteUInt32LittleEndian(headerBuffer.AsSpan(4, 4), chunkSize);

        headerBuffer[8] = (byte)'W'; // Format
        headerBuffer[9] = (byte)'A';
        headerBuffer[10] = (byte)'V';
        headerBuffer[11] = (byte)'E';

        // fmt Subchunk
        headerBuffer[12] = (byte)'f'; // Subchunk1ID
        headerBuffer[13] = (byte)'m';
        headerBuffer[14] = (byte)'t';
        headerBuffer[15] = (byte)' ';

        headerBuffer[16] = 16; // Subchunk1Size (PCM = 16)
        headerBuffer[20] = 1;  // AudioFormat (PCM = 1)

        BinaryPrimitives.WriteUInt16LittleEndian(headerBuffer.AsSpan(22), audioHeader.Channels); // NumChannels
        BinaryPrimitives.WriteUInt32LittleEndian(headerBuffer.AsSpan(24), audioHeader.SampleRate); // SampleRate

        var byteRate = audioHeader.SampleRate * audioHeader.Channels * audioHeader.BitsPerSample / 8;
        BinaryPrimitives.WriteUInt32LittleEndian(headerBuffer.AsSpan(28), byteRate); // ByteRate

        var blockAlign = (ushort)(audioHeader.Channels * audioHeader.BitsPerSample / 8);
        BinaryPrimitives.WriteUInt16LittleEndian(headerBuffer.AsSpan(32), blockAlign); // BlockAlign

        headerBuffer[34] = (byte)audioHeader.BitsPerSample; // BitsPerSample

        // data Subchunk
        headerBuffer[36] = (byte)'d'; // Subchunk2ID
        headerBuffer[37] = (byte)'a';
        headerBuffer[38] = (byte)'t';
        headerBuffer[39] = (byte)'a';

        BinaryPrimitives.WriteUInt32LittleEndian(headerBuffer.AsSpan(40), dataSize); // Subchunk2Size
    }

    /// <summary>
    /// Updates the WAV header with the total size of the audio data.
    /// </summary>
    protected void UpdateHeaderSize()
    {
        var dataSize = totalFramesWritten * audioHeader.Channels * audioHeader.BitsPerSample / 8;
        var chunkSize = 36 + dataSize;

        // Update RIFF chunk size
        BinaryPrimitives.WriteUInt32LittleEndian(headerBuffer.AsSpan(4, 4), (uint)chunkSize);
        // Update data chunk size
        BinaryPrimitives.WriteUInt32LittleEndian(headerBuffer.AsSpan(40), (uint)dataSize);
    }
} 