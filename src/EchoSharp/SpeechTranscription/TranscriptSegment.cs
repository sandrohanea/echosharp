// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;

namespace EchoSharp.SpeechTranscription;

/// <summary>
/// Represents a segment of a transcript.
/// </summary>
public class TranscriptSegment
{
    /// <summary>
    /// Gets or sets the text of the segment.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start time of the segment.
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Gets or sets the duration of the segment
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the confidence of the segment
    /// </summary>
    public float? ConfidenceLevel { get; set; }

    /// <summary>
    /// Gets or sets the language of the segment.
    /// </summary>
    public CultureInfo? Language { get; set; }

    /// <summary>
    /// Gets or sets the tokens of the segment.
    /// </summary>
    /// <remarks>
    /// Not all the transcriptors will provide tokens.
    /// </remarks>
    public IList<TranscriptToken>? Tokens { get; set; }
}
