// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Buffers.Binary;
using EchoSharp.Audio;
using EchoSharp.Tests.Utils;
using Xunit;

namespace EchoSharp.Tests.Audio;

public class BufferedMemoryAudioSourceTests
{
    [Fact]
    public void Initialize_SetsPropertiesCorrectly()
    {
        // Arrange
        var header = new AudioSourceHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        var source = new BufferedMemoryAudioSource();

        // Act
        source.Initialize(header);

        // Assert
        Assert.True(source.IsInitialized);
        Assert.Equal(header.SampleRate, source.SampleRate);
        Assert.Equal(header.Channels, source.ChannelCount);
        Assert.Equal(0, source.FramesCount);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void AddFrame_WithFloats_IncreasesSampleCount(bool storeFloats, bool storeBytes)
    {
        // Arrange
        var header = new AudioSourceHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        var source = new BufferedMemoryAudioSource(storeFloats, storeBytes);
        source.Initialize(header);
        float[] frame = [1f, -1f];

        // Act
        source.AddFrame(frame);

        // Assert
        Assert.Equal(1, source.FramesCount);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void AddFrames_WithBytes_IncreasesSampleCount(bool storeFloats, bool storeBytes)
    {
        // Arrange
        var header = new AudioSourceHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        var source = new BufferedMemoryAudioSource(storeFloats, storeBytes);
        source.Initialize(header);
        byte[] frame = [1, 0, 255, 0];

        // Act
        source.AddFrame(frame);

        // Assert
        Assert.Equal(1, source.FramesCount);
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]

    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    public async Task GetSamplesAsync_ReturnsCorrectFloats(bool storeFloats, bool storeBytes, bool addFloats)
    {
        // Arrange
        var header = new AudioSourceHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        var source = new BufferedMemoryAudioSource(storeFloats, storeBytes);
        source.Initialize(header);
        AddData(source, addFloats);

        // Act
        var samples = await source.GetSamplesAsync(0);
        // Assert

        ArrayAssert.EqualApprox([1f, -1f, 0f, -0.5f, 0.75f, 0.75f], samples.ToArray());
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]

    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    public async Task GetFramesAsync_ReturnsCorrectBytes(bool storeFloats, bool storeBytes, bool addFloats)
    {
        // Arrange
        var header = new AudioSourceHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        var source = new BufferedMemoryAudioSource(storeFloats, storeBytes);
        source.Initialize(header);
        AddData(source, addFloats);

        // Act
        var samples = await source.GetFramesAsync(0);

        // Assert
        Assert.Equal(12, samples.Length);
        float[] expectedFloats = [1, -1, 0, -0.5f, 0.75f, 0.75f];
        for (var i = 0; i < samples.Length; i += 2)
        {
            var sampleShort = BinaryPrimitives.ReadInt16LittleEndian(samples.Span.Slice(i));

            var diff = Math.Abs(expectedFloats[i / 2] - sampleShort / (float)short.MaxValue);
            Assert.InRange(diff, 0f, 0.001f);
        }
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]

    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    public async Task GetSamplesAsync_WithMaxFrames_ReturnsCorrectFloats(bool storeFloats, bool storeBytes, bool addFloats)
    {
        // Arrange
        var header = new AudioSourceHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        var source = new BufferedMemoryAudioSource(storeFloats, storeBytes);
        source.Initialize(header);
        AddData(source, addFloats);

        // Act
        var samples = await source.GetSamplesAsync(0, maxFrames: 2);
        // Assert

        ArrayAssert.EqualApprox([1f, -1f, 0f, -0.5f], samples.ToArray());
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]

    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    public async Task GetSamplesAsync_WithStartFrame_ReturnsCorrectFloats(bool storeFloats, bool storeBytes, bool addFloats)
    {
        // Arrange
        var header = new AudioSourceHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        var source = new BufferedMemoryAudioSource(storeFloats, storeBytes);
        source.Initialize(header);
        AddData(source, addFloats);

        // Act
        var samples = await source.GetSamplesAsync(1);
        // Assert

        ArrayAssert.EqualApprox([0f, -0.5f, 0.75f, 0.75f], samples.ToArray());
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]

    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    public async Task GetFramesAsync_WithStartFrame_ReturnsCorrectBytes(bool storeFloats, bool storeBytes, bool addFloats)
    {
        // Arrange
        var header = new AudioSourceHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        var source = new BufferedMemoryAudioSource(storeFloats, storeBytes);
        source.Initialize(header);
        AddData(source, addFloats);

        // Act
        var samples = await source.GetFramesAsync(1);
        // Assert

        Assert.Equal(8, samples.Length);

        float[] expectedFloats = [0, -0.5f, 0.75f, 0.75f];
        for (var i = 0; i < samples.Length; i += 2)
        {
            var sampleShort = BinaryPrimitives.ReadInt16LittleEndian(samples.Span.Slice(i));

            var diff = Math.Abs(expectedFloats[i / 2] - sampleShort / (float)short.MaxValue);
            Assert.InRange(diff, 0f, 0.001f);
        }
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]

    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    public async Task GetFramesAsync_WithMaxFrames_ReturnsCorrectBytes(bool storeFloats, bool storeBytes, bool addFloats)
    {
        // Arrange
        var header = new AudioSourceHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        var source = new BufferedMemoryAudioSource(storeFloats, storeBytes);
        source.Initialize(header);
        AddData(source, addFloats);

        // Act
        var samples = await source.GetFramesAsync(0, maxFrames: 2);

        // Assert
        Assert.Equal(8, samples.Length);
        float[] expectedFloats = [1, -1, 0, -0.5f];
        for (var i = 0; i < samples.Length; i += 2)
        {
            var sampleShort = BinaryPrimitives.ReadInt16LittleEndian(samples.Span.Slice(i));

            var diff = Math.Abs(expectedFloats[i / 2] - sampleShort / (float)short.MaxValue);
            Assert.InRange(diff, 0f, 0.001f);
        }
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]

    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    public async Task AddFrame_WithAverageAggregationStrategy_AggregatesSamples(bool storeFloats, bool storeBytes, bool addFloats)
    {
        // Arrange
        var header = new AudioSourceHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        var aggregationStrategy = DefaultChannelAggregationStrategies.Average;
        var source = new BufferedMemoryAudioSource(storeFloats, storeBytes, aggregationStrategy: aggregationStrategy);
        source.Initialize(header);
        AddData(source, addFloats);

        // Act
        var samples = await source.GetSamplesAsync(0);

        // Assert
        Assert.Equal(1, source.ChannelCount); // Since aggregation is used
        ArrayAssert.EqualApprox([0f, -0.25f, 0.75f], samples.ToArray()); // Average of 1 and -1, 0 and -2, 0.75 and 0.75
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]

    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    public async Task AddFrame_WithSumAggregationStrategy_AggregatesSamples(bool storeFloats, bool storeBytes, bool addFloats)
    {
        // Arrange
        var header = new AudioSourceHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        var aggregationStrategy = DefaultChannelAggregationStrategies.Sum;
        var source = new BufferedMemoryAudioSource(storeFloats, storeBytes, aggregationStrategy: aggregationStrategy);
        source.Initialize(header);
        AddData(source, addFloats);
        // Act
        var samples = await source.GetSamplesAsync(0);
        // Assert
        Assert.Equal(1, source.ChannelCount); // Since aggregation is used
        ArrayAssert.EqualApprox([0f, -0.5f, 1f], samples.ToArray()); // last element is 1 as we stay in (-1, 1) range
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]

    [InlineData(true, false, true)]
    [InlineData(true, false, false)]
    public async Task AddFrame_WithChannelSelectionAggregationStrategy_AggregatesSamples(bool storeFloats, bool storeBytes, bool addFloats)
    {
        // Arrange
        var header = new AudioSourceHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        var source = new BufferedMemoryAudioSource(storeFloats, storeBytes, aggregationStrategy: DefaultChannelAggregationStrategies.SelectChannel(1));
        source.Initialize(header);
        AddData(source, addFloats);

        // Act
        var samples = await source.GetSamplesAsync(0);

        // Assert
        Assert.Equal(1, source.ChannelCount); // Since aggregation is used
        ArrayAssert.EqualApprox([-1f, -0.5f, 0.75f], samples.ToArray()); // Only second channel
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void Initialize_ThrowsInvalidOperationException_WhenAlreadyInitialized(bool storeFloats, bool storeBytes)
    {
        // Arrange
        var header = new AudioSourceHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        var source = new BufferedMemoryAudioSource(storeFloats, storeBytes);
        source.Initialize(header);
        // Act
        Action act = () => source.Initialize(header);
        // Assert
        var ex = Assert.Throws<InvalidOperationException>(act);
        Assert.Equal("The source is already initialized.", ex.Message);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task MethodsThrowException_WhenNotInitialized(bool storeFloats, bool storeBytes)
    {
        // Arrange
        var source = new BufferedMemoryAudioSource(storeFloats, storeBytes);

        // Act
        Func<Task> getSamplesAsync = async () => await source.GetSamplesAsync(0, 10);
        Action addFrame = () => source.AddFrame(new float[] { 1000f, -1000f });

        // Assert
        var asyncException = await Assert.ThrowsAsync<InvalidOperationException>(getSamplesAsync);
        Assert.Equal("The source is not initialized.", asyncException.Message);

        var syncException = Assert.Throws<InvalidOperationException>(addFrame);
        Assert.Equal("The source is not initialized.", syncException.Message);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void AddFrame_ThrowsArgumentException_WhenFrameSizeMismatch(bool storeFloats, bool storeBytes)
    {
        // Arrange
        var header = new AudioSourceHeader
        {
            Channels = 2,
            SampleRate = 44100,
            BitsPerSample = 16
        };
        var source = new BufferedMemoryAudioSource(storeFloats, storeBytes);
        source.Initialize(header);
        var frame = new float[] { 1000 }.AsMemory(); // Only 1 sample, but channels = 2

        // Act
        Action act = () => source.AddFrame(frame);

        // Assert
        var ex = Assert.Throws<ArgumentException>(act);
        Assert.Matches("The frame size does not match the channels.*", ex.Message);
    }

    [Fact]
    public void Constructor_Throws_IfBothStoreFloatsAndStoreBytesAreFalse()
    {
        // Act
        Action act = static () => _ = new BufferedMemoryAudioSource(storeFloats: false, storeBytes: false);
        // Assert
        var ex = Assert.Throws<ArgumentException>(act);
        Assert.Equal("At least one of storeFloats or storeBytes must be true.", ex.Message);
    }

    private static void AddData(BufferedMemoryAudioSource source, bool addFloats)
    {
        if (addFloats)
        {
            float[] frame1 = [1f, -1f];
            float[] frame2 = [0f, -0.5f];
            float[] frame3 = [0.75f, 0.75f];
            source.AddFrame(frame1);
            source.AddFrame(frame2);
            source.AddFrame(frame3);
            return;
        }

        byte[] frame1Bytes = [0, 0, 0, 0];
        byte[] frame2Bytes = [0, 0, 0, 0];
        byte[] frame3Bytes = [0, 0, 0, 0];
        BinaryPrimitives.WriteInt16LittleEndian(frame1Bytes, short.MaxValue);
        BinaryPrimitives.WriteInt16LittleEndian(frame1Bytes.AsSpan(2), short.MinValue);
        BinaryPrimitives.WriteInt16LittleEndian(frame2Bytes, 0);
        BinaryPrimitives.WriteInt16LittleEndian(frame2Bytes.AsSpan(2), short.MinValue / 2);
        BinaryPrimitives.WriteInt16LittleEndian(frame3Bytes, (short)(short.MaxValue * 0.75f));
        BinaryPrimitives.WriteInt16LittleEndian(frame3Bytes.AsSpan(2), (short)(short.MaxValue * 0.75f));
        source.AddFrame(frame1Bytes);
        source.AddFrame(frame2Bytes);
        source.AddFrame(frame3Bytes);
    }
}
