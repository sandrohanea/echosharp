// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Buffers.Binary;
using EchoSharp.Audio;
using EchoSharp.Audio.Sink;
using EchoSharp.Tests.Utils;
using Xunit;

namespace EchoSharp.Tests.Audio.Sink;

// Test implementation of WaveDecoderSink for testing
public class TestWaveDecoderSink : WaveDecoderSink
{
    private readonly List<float> receivedSamples = [];
    public List<float> ReceivedSamples => receivedSamples;

    public AudioHeader? InitializedHeader { get; private set; }
    public double? InitializedDuration { get; private set; }
    public bool IsInitializedCalled { get; private set; }

    protected override Task InitializeInternalAsync(AudioHeader audioHeader, double? duration)
    {
        InitializedHeader = audioHeader;
        InitializedDuration = duration;
        IsInitializedCalled = true;
        return Task.CompletedTask;
    }

    protected override Task WriteInternalAsync(ReadOnlyMemory<float> samples, CancellationToken cancellationToken)
    {
        receivedSamples.AddRange(samples.ToArray());
        return Task.CompletedTask;
    }
}

public class WaveDecoderSinkTests
{
    [Fact]
    public async Task Initialize_SetsHeaderAndCallsInitializeInternal()
    {
        // Arrange
        var sink = new TestWaveDecoderSink();
        var header = new AudioHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        var duration = 1.5;

        // Act
        await sink.Initialize(header, duration);

        // Assert
        Assert.True(sink.IsInitializedCalled);
        Assert.Equal(header, sink.InitializedHeader);
        Assert.Equal(duration, sink.InitializedDuration);
    }

    [Fact]
    public async Task Initialize_WithSameHeader_DoesNotThrow()
    {
        // Arrange
        var sink = new TestWaveDecoderSink();
        var header = new AudioHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };

        // Act
        await sink.Initialize(header, null);
        await sink.Initialize(header, null); // Should not throw

        // Assert
        Assert.True(sink.IsInitializedCalled);
    }

    [Fact]
    public async Task Initialize_WithDifferentHeader_ThrowsInvalidOperation()
    {
        // Arrange
        var sink = new TestWaveDecoderSink();
        var header1 = new AudioHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };

        var header2 = new AudioHeader
        {
            Channels = 1, // Different channels
            SampleRate = 44100,
            BitsPerSample = 16
        };

        // Act
        await sink.Initialize(header1, null);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => sink.Initialize(header2, null));
    }

    [Fact]
    public async Task WriteAsync_WithSamples_CallsWriteInternal()
    {
        // Arrange
        var sink = new TestWaveDecoderSink();
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
        Assert.Equal(samples, sink.ReceivedSamples);
    }

    [Fact]
    public async Task WriteAsync_WithSamples_ThrowsIfNotInitialized()
    {
        // Arrange
        var sink = new TestWaveDecoderSink();
        var samples = TestData.GetTestSampleData(10, 0.1f);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => sink.WriteAsync(samples));
    }

    [Fact]
    public async Task WriteAsync_WithBytes_ProcessesWavHeader()
    {
        // Arrange
        var sink = new TestWaveDecoderSink();
        var header = new AudioHeader
        {
            Channels = 1,
            SampleRate = 44100,
            BitsPerSample = 16
        };

        // Create a simple WAV header with some sample data
        var wavHeader = new byte[44 + 4]; // 44-byte header + 2 samples

        // RIFF header
        wavHeader[0] = (byte)'R';
        wavHeader[1] = (byte)'I';
        wavHeader[2] = (byte)'F';
        wavHeader[3] = (byte)'F';

        // Chunk size
        BinaryPrimitives.WriteUInt32LittleEndian(wavHeader.AsSpan(4), 36 + 4); // 36 + data size

        // WAVE format
        wavHeader[8] = (byte)'W';
        wavHeader[9] = (byte)'A';
        wavHeader[10] = (byte)'V';
        wavHeader[11] = (byte)'E';

        // fmt subchunk
        wavHeader[12] = (byte)'f';
        wavHeader[13] = (byte)'m';
        wavHeader[14] = (byte)'t';
        wavHeader[15] = (byte)' ';

        wavHeader[16] = 16; // Subchunk1Size (PCM = 16)
        wavHeader[20] = 1;  // AudioFormat (PCM = 1)

        BinaryPrimitives.WriteUInt16LittleEndian(wavHeader.AsSpan(22), header.Channels);
        BinaryPrimitives.WriteUInt32LittleEndian(wavHeader.AsSpan(24), header.SampleRate);

        var byteRate = header.SampleRate * header.Channels * header.BitsPerSample / 8;
        BinaryPrimitives.WriteUInt32LittleEndian(wavHeader.AsSpan(28), byteRate);

        var blockAlign = (ushort)(header.Channels * header.BitsPerSample / 8);
        BinaryPrimitives.WriteUInt16LittleEndian(wavHeader.AsSpan(32), blockAlign);

        wavHeader[34] = (byte)header.BitsPerSample;

        // data subchunk
        wavHeader[36] = (byte)'d';
        wavHeader[37] = (byte)'a';
        wavHeader[38] = (byte)'t';
        wavHeader[39] = (byte)'a';

        BinaryPrimitives.WriteUInt32LittleEndian(wavHeader.AsSpan(40), 4); // 4 bytes of data (2 x 16-bit samples)

        // Sample data - two 16-bit samples (0.5 and -0.5 in normalized float)
        BinaryPrimitives.WriteInt16LittleEndian(wavHeader.AsSpan(44), (short)(0.5f * short.MaxValue));
        BinaryPrimitives.WriteInt16LittleEndian(wavHeader.AsSpan(46), (short)(-0.5f * short.MaxValue));

        // Act
        await sink.WriteAsync(wavHeader);

        // Assert
        Assert.True(sink.IsInitializedCalled);
        Assert.Equal(header.Channels, sink.InitializedHeader?.Channels);
        Assert.Equal(header.SampleRate, sink.InitializedHeader?.SampleRate);
        Assert.Equal(header.BitsPerSample, sink.InitializedHeader?.BitsPerSample);

        // We expect two samples: 0.5 and -0.5 (with small floating point precision differences)
        Assert.Equal(2, sink.ReceivedSamples.Count);
        Assert.True(Math.Abs(sink.ReceivedSamples[0] - 0.5f) < 0.001f);
        Assert.True(Math.Abs(sink.ReceivedSamples[1] - (-0.5f)) < 0.001f);
    }

    [Fact]
    public async Task WriteAsync_WithPartialWavHeader_BuffersUntilComplete()
    {
        // Arrange
        var sink = new TestWaveDecoderSink();
        var header = new AudioHeader
        {
            Channels = 1,
            SampleRate = 44100,
            BitsPerSample = 16
        };

        // Create a simple WAV header with some sample data
        var wavHeader = new byte[44 + 4]; // 44-byte header + 2 samples

        // RIFF header
        wavHeader[0] = (byte)'R';
        wavHeader[1] = (byte)'I';
        wavHeader[2] = (byte)'F';
        wavHeader[3] = (byte)'F';

        // Chunk size
        BinaryPrimitives.WriteUInt32LittleEndian(wavHeader.AsSpan(4), 36 + 4); // 36 + data size

        // WAVE format
        wavHeader[8] = (byte)'W';
        wavHeader[9] = (byte)'A';
        wavHeader[10] = (byte)'V';
        wavHeader[11] = (byte)'E';

        // fmt subchunk
        wavHeader[12] = (byte)'f';
        wavHeader[13] = (byte)'m';
        wavHeader[14] = (byte)'t';
        wavHeader[15] = (byte)' ';

        wavHeader[16] = 16; // Subchunk1Size (PCM = 16)
        wavHeader[20] = 1;  // AudioFormat (PCM = 1)

        BinaryPrimitives.WriteUInt16LittleEndian(wavHeader.AsSpan(22), header.Channels);
        BinaryPrimitives.WriteUInt32LittleEndian(wavHeader.AsSpan(24), header.SampleRate);

        var byteRate = header.SampleRate * header.Channels * header.BitsPerSample / 8;
        BinaryPrimitives.WriteUInt32LittleEndian(wavHeader.AsSpan(28), byteRate);

        var blockAlign = (ushort)(header.Channels * header.BitsPerSample / 8);
        BinaryPrimitives.WriteUInt16LittleEndian(wavHeader.AsSpan(32), blockAlign);

        wavHeader[34] = (byte)header.BitsPerSample;

        // data subchunk
        wavHeader[36] = (byte)'d';
        wavHeader[37] = (byte)'a';
        wavHeader[38] = (byte)'t';
        wavHeader[39] = (byte)'a';

        BinaryPrimitives.WriteUInt32LittleEndian(wavHeader.AsSpan(40), 4); // 4 bytes of data (2 x 16-bit samples)

        // Sample data - two 16-bit samples (0.5 and -0.5 in normalized float)
        BinaryPrimitives.WriteInt16LittleEndian(wavHeader.AsSpan(44), (short)(0.5f * short.MaxValue));
        BinaryPrimitives.WriteInt16LittleEndian(wavHeader.AsSpan(46), (short)(-0.5f * short.MaxValue));

        // Act - Send header in chunks
        await sink.WriteAsync(wavHeader.AsMemory(0, 20)); // First 20 bytes

        // Assert
        Assert.False(sink.IsInitializedCalled); // Not enough data yet

        // Send the rest of the header and data
        await sink.WriteAsync(wavHeader.AsMemory(20));

        // Assert
        Assert.True(sink.IsInitializedCalled);
        Assert.Equal(header.Channels, sink.InitializedHeader?.Channels);
        Assert.Equal(header.SampleRate, sink.InitializedHeader?.SampleRate);
        Assert.Equal(header.BitsPerSample, sink.InitializedHeader?.BitsPerSample);

        // We expect two samples: 0.5 and -0.5 (with small floating point precision differences)
        Assert.Equal(2, sink.ReceivedSamples.Count);
        Assert.True(Math.Abs(sink.ReceivedSamples[0] - 0.5f) < 0.001f);
        Assert.True(Math.Abs(sink.ReceivedSamples[1] - (-0.5f)) < 0.001f);
    }

    [Fact]
    public async Task WriteAsync_AfterInitialize_ProcessesAsRawPcmData()
    {
        // Arrange
        var sink = new TestWaveDecoderSink();
        var header = new AudioHeader
        {
            Channels = 1,
            SampleRate = 44100,
            BitsPerSample = 16
        };

        await sink.Initialize(header, null);

        // Create some 16-bit PCM sample data (0.5 and -0.5 in normalized float)
        var sampleData = new byte[4];
        BinaryPrimitives.WriteInt16LittleEndian(sampleData.AsSpan(0), (short)(0.5f * short.MaxValue));
        BinaryPrimitives.WriteInt16LittleEndian(sampleData.AsSpan(2), (short)(-0.5f * short.MaxValue));

        // Act
        await sink.WriteAsync(sampleData);

        // Assert
        Assert.Equal(2, sink.ReceivedSamples.Count);
        Assert.True(Math.Abs(sink.ReceivedSamples[0] - 0.5f) < 0.001f);
        Assert.True(Math.Abs(sink.ReceivedSamples[1] - (-0.5f)) < 0.001f);
    }

    [Fact]
    public async Task DisposeAsync_ResetsState()
    {
        // Arrange
        var sink = new TestWaveDecoderSink();
        var header = new AudioHeader
        {
            Channels = 1,
            SampleRate = 44100,
            BitsPerSample = 16
        };

        await sink.Initialize(header, null);

        // Act
        await sink.DisposeAsync();

        // This shouldn't throw if we properly disposed
        await sink.DisposeAsync();
    }
}
