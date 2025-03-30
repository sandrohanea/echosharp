// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.SpeechProcessing;

/// <summary>
/// Configuration options for the chunked speech processor.
/// </summary>
public class ChunkedSpeechProcessorOptions
{
    /// <summary>
    /// Gets or sets the maximum number of parallel processing tasks.
    /// </summary>
    /// <remarks>
    /// Default value is the number of processors on the machine.
    /// </remarks>
    public int MaxParallelism { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Gets or sets the padding to apply to each segment.
    /// </summary>
    /// <remarks>
    /// The padding is applied to both the beginning and end of each segment.
    /// </remarks>
    public TimeSpan SegmentPadding { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Gets or sets a value indicating whether to process incomplete segments
    /// </summary>
    /// <remarks>
    /// Incomplete segments are usually the last segment in the source that was not fully finalized.
    /// </remarks>
    public bool ProcessIncompleteSegments { get; set; } = true;
}
