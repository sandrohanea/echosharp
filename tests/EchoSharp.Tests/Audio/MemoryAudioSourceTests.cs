// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Buffers.Binary;
using EchoSharp.Audio.Source;
using EchoSharp.Tests.Utils;
using Xunit;

namespace EchoSharp.Tests.Audio;

public class MemoryAudioSourceTests
{
    [Fact]
    public async Task GetSamples_ReturnsAllChannelsData()
    {
        // Arrange
        var samples = new Memory<float>([1, 2, 3, 4, 5, 6]);

        var audioSource = new MemoryAudioSource(samples, null, 44100, channelCount: 2);
        // Act
        var result = await audioSource.GetSamplesAsync(0);
        // Assert
        Assert.Equal([1, 2, 3, 4, 5, 6], result.Span.ToArray());
    }

    [Fact]
    public void FramesCount_ReturnsCorrectValue()
    {
        // Arrange
        var samples = new Memory<float>([1, 2, 3, 4, 5, 6]);
        var audioSource = new MemoryAudioSource(samples, null, 44100, channelCount: 2);
        // Act
        var result = audioSource.FramesCount;
        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void ChannelCount_ReturnsCorrectValue()
    {
        // Arrange
        var samples = new Memory<float>([1, 2, 3, 4, 5, 6]);
        var audioSource = new MemoryAudioSource(samples, null, 44100, channelCount: 2);
        // Act
        var result = audioSource.ChannelCount;
        // Assert
        Assert.Equal(2, result);
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task GetSamplesAsync_ReturnsAllChannelsData(bool storeFloats, bool storeBytes)
    {
        // Arrange
        float[] floats = [0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f];
        Memory<float>? samples = null;
        if (storeFloats)
        {
            samples = new Memory<float>(floats);
        }

        Memory<byte>? bytes = null;

        if (storeBytes)
        {
            bytes = new Memory<byte>(new byte[floats.Length * 2]);
            for (var i = 0; i < floats.Length; i++)
            {
                var shortValue = (short)(floats[i] * short.MaxValue);
                BinaryPrimitives.WriteInt16LittleEndian(bytes.Value.Span.Slice(i * 2), shortValue);
            }
        }

        var audioSource = new MemoryAudioSource(samples, bytes, 44100, channelCount: 2);
        // Act
        var result = await audioSource.GetSamplesAsync(0);

        // Assert
        Assert.Equal(floats, result.Span.ToArray(), new FloatComparer(FloatComparer.DefaultTolerance));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task GetFrames_ReturnsAllChannelsData(bool storeFloats, bool storeBytes)
    {
        // Arrange
        float[] floats = [0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f];
        Memory<float>? samples = null;
        if (storeFloats)
        {
            samples = new Memory<float>(floats);
        }

        Memory<byte>? bytes = null;

        if (storeBytes)
        {
            bytes = new Memory<byte>(new byte[floats.Length * 2]);
            for (var i = 0; i < floats.Length; i++)
            {
                var shortValue = (short)(floats[i] * short.MaxValue);
                BinaryPrimitives.WriteInt16LittleEndian(bytes.Value.Span.Slice(i * 2), shortValue);
            }
        }

        var audioSource = new MemoryAudioSource(samples, bytes, 44100, channelCount: 2);

        // Act
        var result = await audioSource.GetFramesAsync(0);

        // Assert
        Assert.Equal(3 * 2 * 2, result.Length);
        for (var i = 0; i < 6; i++)
        {
            var shortValue = BinaryPrimitives.ReadInt16LittleEndian(result.Span.Slice(i * 2));
            var floatValue = shortValue / (float)short.MaxValue;
            Assert.True(Math.Abs(floatValue - floats[i]) <= 0.0001f);
        }
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task GetSamplesAsync_WithMaxLengthAndStartIndex_ReturnsAllChannelsData(bool storeFloats, bool storeBytes)
    {
        // Arrange
        float[] floats = [0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f];
        Memory<float>? samples = null;
        if (storeFloats)
        {
            samples = new Memory<float>(floats);
        }

        Memory<byte>? bytes = null;

        if (storeBytes)
        {
            bytes = new Memory<byte>(new byte[floats.Length * 2]);
            for (var i = 0; i < floats.Length; i++)
            {
                var shortValue = (short)(floats[i] * short.MaxValue);
                BinaryPrimitives.WriteInt16LittleEndian(bytes.Value.Span.Slice(i * 2), shortValue);
            }
        }

        var audioSource = new MemoryAudioSource(samples, bytes, 44100, channelCount: 2);
        // Act
        var result = await audioSource.GetSamplesAsync(1, 2);

        // Assert
        Assert.Equal([floats[2], floats[3], floats[4], floats[5]], result.Span.ToArray(), new FloatComparer(FloatComparer.DefaultTolerance));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task GetFrames_WithMaxLengthAndStartIndex_ReturnsAllChannelsData(bool storeFloats, bool storeBytes)
    {
        // Arrange
        float[] floats = [0, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f];
        Memory<float>? samples = null;
        if (storeFloats)
        {
            samples = new Memory<float>(floats);
        }

        Memory<byte>? bytes = null;

        if (storeBytes)
        {
            bytes = new Memory<byte>(new byte[floats.Length * 2]);
            for (var i = 0; i < floats.Length; i++)
            {
                var shortValue = (short)(floats[i] * short.MaxValue);
                BinaryPrimitives.WriteInt16LittleEndian(bytes.Value.Span.Slice(i * 2), shortValue);
            }
        }

        var audioSource = new MemoryAudioSource(samples, bytes, 44100, channelCount: 2);
        // Act
        var result = await audioSource.GetFramesAsync(1, 2);

        // Assert
        Assert.Equal(2 * 2 * 2, result.Length);
        for (var i = 0; i < 4; i++)
        {
            var shortValue = BinaryPrimitives.ReadInt16LittleEndian(result.Span.Slice(i * 2));
            var floatValue = shortValue / (float)short.MaxValue;
            Assert.True(Math.Abs(floatValue - floats[i + 2]) <= 0.0001f);
        }
    }
}
