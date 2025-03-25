// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Buffers.Binary;
using EchoSharp.Audio;
using EchoSharp.Audio.Sink;
using EchoSharp.Tests.Utils;
using Xunit;

namespace EchoSharp.Tests.Audio.Sink;

public class WaveFileSinkTests : IDisposable
{
    private readonly string testFilePath;

    public WaveFileSinkTests()
    {
        testFilePath = Path.Combine(Path.GetTempPath(), $"wave_file_sink_test_{Guid.NewGuid()}.wav");
    }

    public void Dispose()
    {
        // Clean up any test files
        if (File.Exists(testFilePath))
        {
            try
            {
                File.Delete(testFilePath);
            }
            catch
            {
                // Best effort cleanup
            }
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Constructor_CreatesFile_WhenUsingFilePath()
    {
        // Act
        await using var sink = new WaveFileSink(testFilePath);
        await (sink as IAsyncDisposable).DisposeAsync();

        // Assert
        Assert.True(File.Exists(testFilePath));
    }

    [Fact]
    public async Task Initialize_WritesWavHeader_ToTheStream()
    {
        // Arrange
        await using var sink = new WaveFileSink(testFilePath);
        var header = new AudioHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };

        // Act
        await sink.Initialize(header, null);
        await (sink as IAsyncDisposable).DisposeAsync();

        // Assert
        using var fileStream = File.OpenRead(testFilePath);
        var buffer = new byte[44]; // WAV header size

#if NET8_0_OR_GREATER
        var actualRead = await fileStream.ReadAsync(buffer);
#else
        var actualRead = await fileStream.ReadAsync(buffer, 0, 44);
#endif
        Assert.Equal(44, actualRead);
        Assert.Equal('R', (char)buffer[0]); // RIFF header
        Assert.Equal('I', (char)buffer[1]);
        Assert.Equal('F', (char)buffer[2]);
        Assert.Equal('F', (char)buffer[3]);

        Assert.Equal('W', (char)buffer[8]); // WAVE format
        Assert.Equal('A', (char)buffer[9]);
        Assert.Equal('V', (char)buffer[10]);
        Assert.Equal('E', (char)buffer[11]);

        Assert.Equal(16, buffer[16]); // Subchunk1Size (PCM = 16)
        Assert.Equal(1, buffer[20]); // AudioFormat (PCM = 1)

        var channels = BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(22));
        var sampleRate = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(24));
        var bitsPerSample = buffer[34];

        Assert.Equal(header.Channels, channels);
        Assert.Equal(header.SampleRate, sampleRate);
        Assert.Equal(header.BitsPerSample, bitsPerSample);
    }

    [Fact]
    public async Task WriteAsync_WritesSamples_ToTheFile()
    {
        // Arrange
        await using var sink = new WaveFileSink(testFilePath);
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
        await (sink as IAsyncDisposable).DisposeAsync();

        // Assert
        var fileInfo = new FileInfo(testFilePath);
        Assert.True(fileInfo.Length > 44); // Header size + some data

        // Verify that data was written after the 44-byte header
        using var fileStream = File.OpenRead(testFilePath);
        var buffer = new byte[44 + samples.Length * 2]; // Header + 16-bit samples

#if NET8_0_OR_GREATER
        var actualRead = await fileStream.ReadAsync(buffer);
#else
        var actualRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
#endif
        Assert.Equal(buffer.Length, actualRead);
        var dataSize = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(40));
        Assert.Equal((uint)(samples.Length * 2), dataSize); // 2 bytes per sample for 16-bit
    }

    [Fact]
    public async Task WriteAsync_WithBytes_WritesToTheFile()
    {
        // Arrange
        await using var sink = new WaveFileSink(testFilePath);
        var header = new AudioHeader
        {
            Channels = 1,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        await sink.Initialize(header, null);

        var testBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        // Act
        await sink.WriteAsync(testBytes);
        await (sink as IAsyncDisposable).DisposeAsync();

        // Assert
        var fileInfo = new FileInfo(testFilePath);
        Assert.Equal(44 + testBytes.Length, fileInfo.Length); // Header size + data
    }

    [Fact]
    public async Task Initialize_ThrowsInvalidOperationException_WhenAlreadyInitialized()
    {
        // Arrange
        await using var sink = new WaveFileSink(testFilePath);
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
    public async Task WriteAsync_ThrowsInvalidOperationException_WhenNotInitialized()
    {
        // Arrange
        await using var sink = new WaveFileSink(testFilePath);
        var samples = TestData.GetTestSampleData(10, 0.1f);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => sink.WriteAsync(samples));
    }

    [Fact]
    public async Task WriteAsync_ThrowsArgumentException_WhenSamplesDontMatchChannels()
    {
        // Arrange
        await using var sink = new WaveFileSink(testFilePath);
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
    public async Task Constructor_WithStream_UsesProvidedStream()
    {
        // Arrange
        using var memoryStream = new MemoryStream();

        // Act
        await using var sink = new WaveFileSink(memoryStream, leaveOpen: true);
        var header = new AudioHeader
        {
            Channels = 1,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        await sink.Initialize(header, null);

        var samples = TestData.GetTestSampleData(10, 0.1f);
        await sink.WriteAsync(samples);
        await (sink as IAsyncDisposable).DisposeAsync();

        // Assert
        Assert.True(memoryStream.Length > 44); // Header + data
        Assert.True(memoryStream.CanRead); // Stream still open due to leaveOpen: true
    }
}
