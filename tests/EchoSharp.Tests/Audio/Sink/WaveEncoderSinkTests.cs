// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Buffers.Binary;
using EchoSharp.Audio;
using EchoSharp.Audio.Sink;
using EchoSharp.Tests.Utils;
using Xunit;

namespace EchoSharp.Tests.Audio.Sink;

// Test implementation of WaveEncoderSink for testing
public class TestWaveEncoderSink : WaveEncoderSink
{
    private readonly MemoryStream memoryStream = new();
    public MemoryStream OutputStream => memoryStream;

    public override Task WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        return memoryStream.WriteAsync(buffer, cancellationToken).AsTask();
#else
        return memoryStream.WriteAsync(buffer.ToArray(), 0, buffer.Length);
#endif
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        try
        {
            UpdateHeaderSize();
            memoryStream.Position = 0;
#if NET8_0_OR_GREATER
            await memoryStream.WriteAsync(headerBuffer.AsMemory(0, 44));
#else
            await memoryStream.WriteAsync(headerBuffer, 0, 44);
#endif
            await memoryStream.FlushAsync();
        }
        finally
        {
            memoryStream.Dispose();
        }

        return;
    }
}

public class WaveEncoderSinkTests
{
    [Fact]
    public async Task Initialize_SetsAudioHeaderAndWritesWavHeader()
    {
        // Arrange
        var sink = new TestWaveEncoderSink();
        var header = new AudioHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };

        // Act
        await sink.Initialize(header, null);

        // Assert
        Assert.Equal(44, sink.OutputStream.Length); // WAV header size

        // Read the header to verify
        sink.OutputStream.Position = 0;
        var buffer = new byte[44];
#if NET8_0_OR_GREATER
        await sink.OutputStream.ReadAsync(buffer);
#else
        await sink.OutputStream.ReadAsync(buffer, 0, 44);
#endif

        Assert.Equal('R', (char)buffer[0]); // RIFF header
        Assert.Equal('I', (char)buffer[1]);
        Assert.Equal('F', (char)buffer[2]);
        Assert.Equal('F', (char)buffer[3]);

        Assert.Equal('W', (char)buffer[8]); // WAVE format
        Assert.Equal('A', (char)buffer[9]);
        Assert.Equal('V', (char)buffer[10]);
        Assert.Equal('E', (char)buffer[11]);

        var channels = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(22));
        var sampleRate = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(24));
        var bitsPerSample = buffer[34];

        Assert.Equal(header.Channels, channels);
        Assert.Equal(header.SampleRate, sampleRate);
        Assert.Equal(header.BitsPerSample, bitsPerSample);
    }

    [Fact]
    public async Task Initialize_WithDuration_SetsCorrectChunkSizes()
    {
        // Arrange
        var sink = new TestWaveEncoderSink();
        var header = new AudioHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        var duration = 1.0; // 1 second

        // Act
        await sink.Initialize(header, duration);

        // Assert
        sink.OutputStream.Position = 0;
        var buffer = new byte[44];
#if NET8_0_OR_GREATER
        await sink.OutputStream.ReadAsync(buffer);
#else
        await sink.OutputStream.ReadAsync(buffer, 0, 44);
#endif

        // 1 second of stereo 44.1kHz 16-bit audio = 44100 * 2 * 2 = 176400 bytes
        var expectedDataSize = (uint)(duration * header.SampleRate * header.Channels * (header.BitsPerSample / 8));
        var expectedChunkSize = 36 + expectedDataSize;

        var chunkSize = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(4));
        var dataSize = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(40));

        Assert.Equal(expectedDataSize, dataSize);
        Assert.Equal(expectedChunkSize, chunkSize);
    }

    [Fact]
    public async Task Initialize_ThrowsInvalidOperationException_WhenAlreadyInitialized()
    {
        // Arrange
        var sink = new TestWaveEncoderSink();
        var header = new AudioHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        await sink.Initialize(header, null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => sink.Initialize(header, null));
    }

    [Fact]
    public async Task WriteAsync_WithSamples_WritesCorrectBytesToStream()
    {
        // Arrange
        var sink = new TestWaveEncoderSink();
        var header = new AudioHeader
        {
            Channels = 1,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        await sink.Initialize(header, null);
        var samples = TestData.GetTestSampleData(10, 0.1f);

        // Act
        await sink.WriteAsync(samples);

        // Assert
        Assert.Equal(44 + samples.Length * 2, sink.OutputStream.Length); // Header + samples (16-bit = 2 bytes per sample)
    }

    [Fact]
    public async Task WriteAsync_ThrowsArgumentException_WhenSamplesDontMatchChannels()
    {
        // Arrange
        var sink = new TestWaveEncoderSink();
        var header = new AudioHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        await sink.Initialize(header, null);
        var samples = TestData.GetTestSampleData(3, 0.1f); // Not a multiple of 2 channels

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => sink.WriteAsync(samples));
    }

    [Fact]
    public async Task WriteAsync_ThrowsInvalidOperationException_WhenNotInitialized()
    {
        // Arrange
        var sink = new TestWaveEncoderSink();
        var samples = TestData.GetTestSampleData(10, 0.1f);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => sink.WriteAsync(samples));
    }

    [Fact]
    public async Task DisposeAsync_UpdatesHeaderWithCorrectSize()
    {
        // Arrange
        var sink = new TestWaveEncoderSink();
        var header = new AudioHeader
        {
            Channels = 1,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        await sink.Initialize(header, null);
        var samples = TestData.GetTestSampleData(10, 0.1f);
        await sink.WriteAsync(samples);

        // Act
        await sink.DisposeAsync();

        // Assert
        var buffer = sink.OutputStream.ToArray();
        var dataSize = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(40));
        var chunkSize = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(4));

        Assert.Equal((uint)(samples.Length * 2), dataSize); // 16-bit = 2 bytes per sample
        Assert.Equal(36u + dataSize, chunkSize); // 36 bytes of header info + data size
    }
}
