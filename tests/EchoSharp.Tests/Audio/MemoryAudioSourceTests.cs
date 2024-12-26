// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Buffers.Binary;
using FluentAssertions;
using EchoSharp.Audio;
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
        result.Span.ToArray().Should().Equal(1, 2, 3, 4, 5, 6);
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
        result.Should().Be(3); // 6 samples / 2 channels = 3 frames
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
        result.Should().Be(2);
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
        result.Span.ToArray().Should().BeApproxEqual(floats);
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
        result.Length.Should().Be(3 * 2 * 2); // 2 frames with 2 channels and 2 bytes each

        for (var i = 0; i < 6; i++)
        {
            var shortValue = BinaryPrimitives.ReadInt16LittleEndian(result.Span.Slice(i * 2));
            var floatValue = shortValue / (float)short.MaxValue;
            floatValue.Should().BeApproximately(floats[i], 0.0001f);
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
        result.Span.ToArray().Should().BeApproxEqual([floats[2], floats[3], floats[4], floats[5]]);
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
        result.Length.Should().Be(2 * 2 * 2); // 2 frames with 2 channels and 2 bytes each

        for (var i = 0; i < 4; i++)
        {
            var shortValue = BinaryPrimitives.ReadInt16LittleEndian(result.Span.Slice(i * 2));
            var floatValue = shortValue / (float)short.MaxValue;
            floatValue.Should().BeApproximately(floats[i + 2], 0.0001f);
        }
    }
}
