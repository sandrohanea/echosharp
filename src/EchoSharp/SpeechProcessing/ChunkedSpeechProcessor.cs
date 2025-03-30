// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using EchoSharp.Audio.Source;
using EchoSharp.VoiceActivityDetection;

namespace EchoSharp.SpeechProcessing;

/// <summary>
/// A speech processor that chunks the audio source into smaller segments using a VAD detector
/// and processes each segment in parallel using an underlying speech processor.
/// </summary>
public class ChunkedSpeechProcessor : ISpeechProcessor
{
    private readonly ISpeechProcessorFactory innerProcessorFactory;
    private readonly SpeechProcessorOptions innerProcessorOptions;
    private readonly IVadDetector vadDetector;
    private readonly ChunkedSpeechProcessorOptions options;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChunkedSpeechProcessor"/> class.
    /// </summary>
    /// <param name="innerProcessorFactory">The underlying speech processor factory to use for processing individual segments.</param>
    /// <param name="innerProcessorOptions">The inner processor options to be used when creating processors.</param>
    /// <param name="vadDetector">The VAD detector to use for segmenting the audio.</param>
    /// <param name="options">The options for the chunked speech processor.</param>
    public ChunkedSpeechProcessor(
        ISpeechProcessorFactory innerProcessorFactory,
        SpeechProcessorOptions innerProcessorOptions,
        IVadDetector vadDetector,
        ChunkedSpeechProcessorOptions options)
    {
        this.innerProcessorFactory = innerProcessorFactory ?? throw new ArgumentNullException(nameof(innerProcessorFactory));
        this.innerProcessorOptions = innerProcessorOptions;
        this.vadDetector = vadDetector ?? throw new ArgumentNullException(nameof(vadDetector));
        this.options = options ?? new ChunkedSpeechProcessorOptions();
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<ProcessedSpeechSegment> TranscribeAsync(IAudioSource source, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var segments = new List<VadSegment>();
        await foreach (var segment in GetVadSegmentsAsync(source, cancellationToken))
        {
            segments.Add(segment);
        }

        if (segments.Count == 0)
        {
            // If no segments were detected we just return without returning any event
            yield break;
        }

        var results = new ConcurrentBag<(ProcessedSpeechSegment Segment, int SegmentIndex)>();

        // Create a list of tasks to process each segment
        var tasks = new List<Task>();
        var maxConcurrency = Math.Min(options.MaxParallelism, segments.Count);
        var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

        // Create a task for each segment
        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];
            var index = i;

            // Create a task that processes the segment
            var task = ProcessSegmentAsync(segment, index);
            tasks.Add(task);

            // Local function to process a segment with semaphore for controlling concurrency
            async Task ProcessSegmentAsync(VadSegment segmentToProcess, int segmentIndex)
            {
                try
                {
                    await semaphore.WaitAsync(cancellationToken);

                    var slicedSource = new SliceAudioSource(source, segmentToProcess.StartTime, segmentToProcess.Duration);
                    using var innerProcessor = innerProcessorFactory.Create(innerProcessorOptions);
                    await foreach (var processedSegment in innerProcessor.TranscribeAsync(slicedSource, cancellationToken))
                    {
                        // Adjust the start time based on the original segment start time
                        var adjustedSegment = new ProcessedSpeechSegment
                        {
                            StartTime = segmentToProcess.StartTime + processedSegment.StartTime,
                            Duration = processedSegment.Duration,
                            Text = processedSegment.Text,
                            Language = processedSegment.Language,
                            ConfidenceLevel = processedSegment.ConfidenceLevel,
                            Tokens = processedSegment.Tokens
                        };

                        results.Add((adjustedSegment, segmentIndex));
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }
        }

        // Wait for all tasks to complete
        await Task.WhenAll(tasks);

        // Yield the results in the original segment order
        foreach (var result in results.OrderBy(r => r.SegmentIndex).ThenBy(r => r.Segment.StartTime))
        {
            yield return result.Segment;
        }
    }

    private async IAsyncEnumerable<VadSegment> GetVadSegmentsAsync(IAudioSource source, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var segment in vadDetector.DetectSegmentsAsync(source, cancellationToken))
        {
            // Skip incomplete segments as they might not contain valid speech
            if (segment.IsIncomplete && !options.ProcessIncompleteSegments)
            {
                continue;
            }

            // Apply padding to the segment if configured
            if (options.SegmentPadding > TimeSpan.Zero)
            {
                yield return PadSegment(segment, source.Duration);
            }
            else
            {
                yield return segment;
            }
        }
    }

    private VadSegment PadSegment(VadSegment segment, TimeSpan totalDuration)
    {
        var halfPadding = TimeSpan.FromTicks(options.SegmentPadding.Ticks / 2);

        // Calculate new start time with padding
        var newStartTime = segment.StartTime - halfPadding;
        if (newStartTime < TimeSpan.Zero)
        {
            newStartTime = TimeSpan.Zero;
        }

        // Calculate new duration with padding
        var availableEndTime = Math.Min(
            totalDuration.TotalMilliseconds,
            (segment.StartTime + segment.Duration + halfPadding).TotalMilliseconds);

        var newDuration = TimeSpan.FromMilliseconds(availableEndTime - newStartTime.TotalMilliseconds);

        var paddedSegment = new VadSegment
        {
            StartTime = newStartTime,
            Duration = newDuration,
            IsIncomplete = segment.IsIncomplete
        };

        return paddedSegment;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        innerProcessorFactory.Dispose();
        vadDetector.Dispose();
        isDisposed = true;
    }
}
