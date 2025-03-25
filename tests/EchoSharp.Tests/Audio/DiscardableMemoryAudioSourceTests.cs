// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Audio;
using EchoSharp.Audio.Source;
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
        discardableSource.Initialize(new AudioHeader()
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
        Assert.Equal(TestData.GetTestSampleData(6, 0.07f), samples.Span.ToArray(), new FloatComparer(FloatComparer.DefaultTolerance));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void DiscardFrames_WhenMoreSamplesAreDiscardedThanExists_Throws(bool storeFloats, bool storeBytes)
    {
        // Arrange
        var discardableSource = new DiscardableMemoryAudioSource(storeFloats, storeBytes);
        discardableSource.Initialize(new AudioHeader()
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
        Assert.Throws<ArgumentOutOfRangeException>(() => discardableSource.DiscardFrames(7));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void DiscardFrames_WhenCountIsZero_Throws(bool storeFloats, bool storeBytes)
    {
        // Arrange
        var discardableSource = new DiscardableMemoryAudioSource(storeFloats, storeBytes);
        discardableSource.Initialize(new AudioHeader()
        {
            BitsPerSample = 16,
            Channels = 2,
            SampleRate = 44100
        });
        // Act
        Assert.Throws<ArgumentOutOfRangeException>(() => discardableSource.DiscardFrames(0));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void DiscardFrames_WhenCountIsNegative_Throws(bool storeFloats, bool storeBytes)
    {
        // Arrange
        var discardableSource = new DiscardableMemoryAudioSource(storeFloats, storeBytes);
        discardableSource.Initialize(new AudioHeader()
        {
            BitsPerSample = 16,
            Channels = 2,
            SampleRate = 44100
        });
        // Act
        Assert.Throws<ArgumentOutOfRangeException>(() => discardableSource.DiscardFrames(-1));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task GetSamplesAsync_WhenStartFrameIsLessThanDiscardedFrames_Throws(bool storeFloats, bool storeBytes)
    {
        // Arrange
        var discardableSource = new DiscardableMemoryAudioSource(storeFloats, storeBytes);
        discardableSource.Initialize(new AudioHeader()
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
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => discardableSource.GetSamplesAsync(2));
    }
}
