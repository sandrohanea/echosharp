// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Audio.Source;
using EchoSharp.SpeechProcessing;
using EchoSharp.Tests.Utils;
using EchoSharp.VoiceActivityDetection;
using NSubstitute;
using Xunit;

namespace EchoSharp.Tests.SpeechProcessing;

public class ChunkedSpeechProcessorTests
{
    [Fact]
    public async Task TranscribeAsync_WithNoSegments_ReturnsEmptyEnumerable()
    {
        // Arrange
        var innerProcessor = Substitute.For<ISpeechProcessor>();
        var innerProcessorFactory = Substitute.For<ISpeechProcessorFactory>();
        var vadDetector = Substitute.For<IVadDetector>();
        var source = Substitute.For<IAudioSource>();
        var options = new ChunkedSpeechProcessorOptions
        {
            MaxParallelism = 2
        };

        var expectedSegment = new ProcessedSpeechSegment
        {
            StartTime = TimeSpan.FromSeconds(1),
            Duration = TimeSpan.FromSeconds(2),
            Text = "Test transcript",
            ConfidenceLevel = 0.9f
        };

        vadDetector
            .DetectSegmentsAsync(Arg.Any<IAudioSource>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<VadSegment>().ToAsyncEnumerable());

        innerProcessor
            .TranscribeAsync(Arg.Any<IAudioSource>(), Arg.Any<CancellationToken>())
            .Returns(new[] { expectedSegment }.ToAsyncEnumerable());

        innerProcessorFactory
            .Create(Arg.Any<SpeechProcessorOptions>())
            .Returns(innerProcessor);

        var processor = new ChunkedSpeechProcessor(innerProcessorFactory, new SpeechProcessorOptions(), vadDetector, options);

        // Act
        var results = await processor.TranscribeAsync(source, CancellationToken.None).ToListAsync();

        // Assert
        Assert.Empty(results);
        innerProcessor.Received(0).TranscribeAsync(source, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TranscribeAsync_WithMultipleSegments_ProcessesEachSegment()
    {
        // Arrange
        var innerProcessor = Substitute.For<ISpeechProcessor>();
        var innerProcessorFactory = Substitute.For<ISpeechProcessorFactory>();
        var vadDetector = Substitute.For<IVadDetector>();
        var source = Substitute.For<IAudioSource>();
        var options = new ChunkedSpeechProcessorOptions
        {
            MaxParallelism = 2
        };

        source.Duration.Returns(TimeSpan.FromSeconds(10));

        var vadSegments = new List<VadSegment>
        {
            new() { StartTime = TimeSpan.FromSeconds(1), Duration = TimeSpan.FromSeconds(2) },
            new() { StartTime = TimeSpan.FromSeconds(5), Duration = TimeSpan.FromSeconds(2) }
        };

        vadDetector
            .DetectSegmentsAsync(Arg.Any<IAudioSource>(), Arg.Any<CancellationToken>())
            .Returns(vadSegments.ToAsyncEnumerable());

        // Setup inner processor - since we can't easily match on SliceAudioSource properties,
        // we'll use Arg.Any and configure the responses for any source
        innerProcessor
            .TranscribeAsync(Arg.Any<IAudioSource>(), Arg.Any<CancellationToken>())
            .Returns(
                callInfo =>
                {
                    // Return different segments based on the sequence of calls
                    // This is a simplified approach as we're not checking the exact SliceAudioSource properties
                    return new[]
                    {
                        new ProcessedSpeechSegment
                        {
                            StartTime = TimeSpan.Zero,
                            Duration = TimeSpan.FromSeconds(2),
                            Text = "First segment",
                            ConfidenceLevel = 0.9f
                        }
                    }.ToAsyncEnumerable();
                },
                callInfo =>
                {
                    return new[]
                    {
                        new ProcessedSpeechSegment
                        {
                            StartTime = TimeSpan.Zero,
                            Duration = TimeSpan.FromSeconds(2),
                            Text = "Second segment",
                            ConfidenceLevel = 0.9f
                        }
                    }.ToAsyncEnumerable();
                }
            );

        innerProcessorFactory
            .Create(Arg.Any<SpeechProcessorOptions>())
            .Returns(innerProcessor);

        var processor = new ChunkedSpeechProcessor(innerProcessorFactory, new SpeechProcessorOptions(), vadDetector, options);

        // Act
        var results = await processor.TranscribeAsync(source, CancellationToken.None).ToListAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("First segment", results[0].Text);
        Assert.Equal(TimeSpan.FromSeconds(1), results[0].StartTime); // Adjusted start time
        Assert.Equal("Second segment", results[1].Text);
        Assert.Equal(TimeSpan.FromSeconds(5), results[1].StartTime); // Adjusted start time
    }

    [Fact]
    public async Task TranscribeAsync_WithIncompleteSegments_FiltersBasedOnOptions()
    {
        // Arrange
        var innerProcessor = Substitute.For<ISpeechProcessor>();
        var innerProcessorFactory = Substitute.For<ISpeechProcessorFactory>();
        var vadDetector = Substitute.For<IVadDetector>();
        var source = Substitute.For<IAudioSource>();

        // Don't process incomplete segments
        var options = new ChunkedSpeechProcessorOptions
        {
            MaxParallelism = 2,
            ProcessIncompleteSegments = false
        };

        source.Duration.Returns(TimeSpan.FromSeconds(10));

        var vadSegments = new List<VadSegment>
        {
            new() { StartTime = TimeSpan.FromSeconds(1), Duration = TimeSpan.FromSeconds(2), IsIncomplete = false },
            new() { StartTime = TimeSpan.FromSeconds(5), Duration = TimeSpan.FromSeconds(2), IsIncomplete = true }
        };

        vadDetector
            .DetectSegmentsAsync(Arg.Any<IAudioSource>(), Arg.Any<CancellationToken>())
            .Returns(vadSegments.ToAsyncEnumerable());

        // Setup inner processor
        innerProcessor
            .TranscribeAsync(Arg.Any<IAudioSource>(), Arg.Any<CancellationToken>())
            .Returns(new[]
            {
                new ProcessedSpeechSegment
                {
                    StartTime = TimeSpan.Zero,
                    Duration = TimeSpan.FromSeconds(2),
                    Text = "Test segment",
                    ConfidenceLevel = 0.9f
                }
            }.ToAsyncEnumerable());

        innerProcessorFactory
            .Create(Arg.Any<SpeechProcessorOptions>())
            .Returns(innerProcessor);

        var processor = new ChunkedSpeechProcessor(innerProcessorFactory, new SpeechProcessorOptions(), vadDetector, options);

        // Act
        var results = await processor.TranscribeAsync(source, CancellationToken.None).ToListAsync();

        // Assert
        Assert.Single(results); // Only the complete segment should be processed
        Assert.Equal(TimeSpan.FromSeconds(1), results[0].StartTime);
    }

    [Fact]
    public void Dispose_DisposesInnerComponentsOnce()
    {
        // Arrange
        var innerProcessorFactory = Substitute.For<ISpeechProcessorFactory>();
        var vadDetector = Substitute.For<IVadDetector>();
        var options = new ChunkedSpeechProcessorOptions();

        var processor = new ChunkedSpeechProcessor(innerProcessorFactory, new SpeechProcessorOptions(), vadDetector, options);

        // Act
        processor.Dispose();
        processor.Dispose(); // Second call should not call Dispose again

        // Assert
        innerProcessorFactory.Received(1).Dispose();
        vadDetector.Received(1).Dispose();
    }
}
