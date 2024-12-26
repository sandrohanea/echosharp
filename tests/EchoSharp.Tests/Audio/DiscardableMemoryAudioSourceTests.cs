// Licensed under the MIT license: https://opensource.org/licenses/MIT

using FluentAssertions;
using EchoSharp.Audio;
using EchoSharp.Tests.Utils;
using Xunit;

namespace EchoSharp.Tests.Audio;

public class DiscardableMemoryAudioSourceTests
{
    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task DiscardFrames_RemovesTheSpecifiedNumberOfFrames(bool storeFloats, bool storeBytes)
    {
        // Arrange
        var discardableSource = new DiscardableMemoryAudioSource(storeFloats, storeBytes);
        discardableSource.Initialize(new AudioSourceHeader()
        {
            BitsPerSample = 16,
            Channels = 2,
            SampleRate = 44100
        });

        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.01f));
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.03f));
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.05f));
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.07f));
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.09f));
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.11f));
        // we have 6 frames

        // Act
        discardableSource.DiscardFrames(3);

        // Assert
        Assert.Equal(3, discardableSource.FramesCount);
        var samples = await discardableSource.GetSamplesAsync(3);
        samples.Span.ToArray().Should().BeApproxEqual(TestData.GetTestSampleData(6, 0.07f));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void DiscardFrames_WhenMoreSamplesAreDiscardedThanExists_Throws(bool storeFloats, bool storeBytes)
    {
        // Arrange
        var discardableSource = new DiscardableMemoryAudioSource(storeFloats, storeBytes);
        discardableSource.Initialize(new AudioSourceHeader()
        {
            BitsPerSample = 16,
            Channels = 2,
            SampleRate = 44100
        });
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.01f));
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.03f));
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.05f));
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.07f));
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.09f));
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.11f));
        // we have 6 frames
        // Act
        Action act = () => discardableSource.DiscardFrames(7);
        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void DiscardFrames_WhenCountIsZero_Throws(bool storeFloats, bool storeBytes)
    {
        // Arrange
        var discardableSource = new DiscardableMemoryAudioSource(storeFloats, storeBytes);
        discardableSource.Initialize(new AudioSourceHeader()
        {
            BitsPerSample = 16,
            Channels = 2,
            SampleRate = 44100
        });
        // Act
        Action act = () => discardableSource.DiscardFrames(0);
        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void DiscardFrames_WhenCountIsNegative_Throws(bool storeFloats, bool storeBytes)
    {
        // Arrange
        var discardableSource = new DiscardableMemoryAudioSource(storeFloats, storeBytes);
        discardableSource.Initialize(new AudioSourceHeader()
        {
            BitsPerSample = 16,
            Channels = 2,
            SampleRate = 44100
        });
        // Act
        Action act = () => discardableSource.DiscardFrames(-1);
        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void GetSamplesAsync_WhenStartFrameIsLessThanDiscardedFrames_Throws(bool storeFloats, bool storeBytes)
    {
        // Arrange
        var discardableSource = new DiscardableMemoryAudioSource(storeFloats, storeBytes);
        discardableSource.Initialize(new AudioSourceHeader()
        {
            BitsPerSample = 16,
            Channels = 2,
            SampleRate = 44100
        });
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.01f));
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.03f));
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.05f));
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.07f));
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.09f));
        discardableSource.AddFrame(TestData.GetTestSampleData(2, 0.11f));
        discardableSource.DiscardFrames(3);
        // Act
        Action act = () => discardableSource.GetSamplesAsync(2);
        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
