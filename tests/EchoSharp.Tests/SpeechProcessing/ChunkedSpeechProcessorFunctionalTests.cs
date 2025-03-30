// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;
using System.Runtime.CompilerServices;
using EchoSharp.Audio.Source;
using EchoSharp.SpeechProcessing;
using EchoSharp.Tests.Utils;
using EchoSharp.VoiceActivityDetection;
using Xunit;

namespace EchoSharp.Tests.SpeechProcessing;

public class ChunkedSpeechProcessorFunctionalTests(FunctionalTestFixture fixture) : IClassFixture<FunctionalTestFixture>
{
    [Fact]
    public async Task TranscribeAsync_WithRealAudioFile_ChunksAndProcessesCorrectly()
    {
        // Arrange
        var file = "./files/testFile.wav";

        using var fileStream = File.OpenRead(file);
        using var fileSource = new WaveFileAudioSource(fileStream);

        // Initialize the source
        await fileSource.InitializeAsync();

        // Create VAD detector with short segments to ensure multiple segments are created
        var vadOptions = new VadDetectorOptions
        {
            MinSilenceDuration = TimeSpan.FromMilliseconds(100),
            MinSpeechDuration = TimeSpan.FromMilliseconds(100)
        };

        // Create the VAD detector factory instead of trying to access the internal implementation
        var vadFactory = await fixture.GetSileroVadDetectorFactoryAsync();
        var vadDetector = vadFactory.CreateVadDetector(vadOptions);

        // Create a simple speech processor factory for testing
        var whisperFactory = await fixture.GetWhisperProcessorFactoryAsync();

        // Set chunked processor options with max parallelism and padding
        var options = new ChunkedSpeechProcessorOptions
        {
            MaxParallelism = 4,
            SegmentPadding = TimeSpan.FromMilliseconds(50),
            ProcessIncompleteSegments = true
        };

        // Create the chunked processor
        using var chunkedProcessor = new ChunkedSpeechProcessor(
            whisperFactory,
            new SpeechProcessorOptions(),
            vadDetector,
            options);

        // Act
        var results = await chunkedProcessor
            .TranscribeAsync(fileSource, CancellationToken.None)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(results);

        // Verify all segments have text
        foreach (var segment in results)
        {
            Assert.NotNull(segment.Text);
            Assert.NotEmpty(segment.Text);
        }

        // Verify segments have different start times
        var startTimes = results.Select(s => s.StartTime).Distinct().Count();
        Assert.True(startTimes > 1, "Expected multiple segments with different start times");

        // Verify that segments are ordered by start time
        var orderedResults = results.OrderBy(s => s.StartTime).ToList();
        Assert.Equal(results, orderedResults);
    }

    [Fact]
    public async Task WithChunking_ExtensionMethod_WorksCorrectly()
    {
        // Arrange
        var file = "./files/testFile.wav";

        using var fileStream = File.OpenRead(file);
        using var fileSource = new WaveFileAudioSource(fileStream);

        // Initialize the source
        await fileSource.InitializeAsync();

        // Create the original speech processor factory
        var speechProcessorFactory = new SimpleSpeechProcessorFactory();

        // Create VAD detector factory
        var vadFactory = await fixture.GetSileroVadDetectorFactoryAsync();

        // Configure VAD options
        var vadOptions = new VadDetectorOptions
        {
            MinSilenceDuration = TimeSpan.FromMilliseconds(150),
            MinSpeechDuration = TimeSpan.FromMilliseconds(150)
        };

        // Configure chunking options
        var chunkOptions = new ChunkedSpeechProcessorOptions
        {
            MaxParallelism = 2,
            SegmentPadding = TimeSpan.FromMilliseconds(75),
            ProcessIncompleteSegments = true
        };

        // Use the extension method to create a chunked factory
        using var chunkedFactory = speechProcessorFactory.WithChunking(
            vadFactory,
            vadOptions,
            chunkOptions);

        // Create a speech processor from the factory
        using var processor = chunkedFactory.Create(new SpeechProcessorOptions());

        // Act
        var results = await processor
            .TranscribeAsync(fileSource, CancellationToken.None)
            .ToListAsync();

        // Assert
        Assert.NotEmpty(results);

        // Verify all segments have text
        foreach (var segment in results)
        {
            Assert.NotNull(segment.Text);
            Assert.NotEmpty(segment.Text);
        }

        // Verify segments have different start times (VAD is finding segments)
        var startTimes = results.Select(s => s.StartTime).Distinct().Count();
        Assert.True(startTimes > 1, "Expected multiple segments with different start times");
    }

    /// <summary>
    /// Simple implementation of ISpeechProcessor for testing purposes.
    /// It echoes back text based on the timestamp of the audio segment.
    /// </summary>
    private class SimpleSpeechProcessor : ISpeechProcessor
    {
        public async IAsyncEnumerable<ProcessedSpeechSegment> TranscribeAsync(
            IAudioSource source,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            // Simple transcription that just returns the segment info
            var segment = new ProcessedSpeechSegment
            {
                StartTime = TimeSpan.Zero, // We don't try to access internal SliceAudioSource properties
                Duration = source.Duration,
                Text = $"Speech at {source.Duration.TotalSeconds:F1} seconds",
                ConfidenceLevel = 0.95f,
                Language = CultureInfo.InvariantCulture
            };

            yield return segment;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }

    /// <summary>
    /// Simple factory to create SimpleSpeechProcessor instances
    /// </summary>
    private class SimpleSpeechProcessorFactory : ISpeechProcessorFactory
    {
        public ISpeechProcessor Create(SpeechProcessorOptions options)
        {
            return new SimpleSpeechProcessor();
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
