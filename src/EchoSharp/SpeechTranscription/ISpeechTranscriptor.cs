// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Audio;

namespace EchoSharp.SpeechTranscription;

/// <summary>
/// Represents a segment of transcribed text.
/// </summary>
public interface ISpeechTranscriptor : IDisposable
{
    /// <summary>
    /// Transcribes the given audio stream to segments of text.
    /// </summary>
    /// <param name="source">The audio source to transcribe.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns></returns>
    IAsyncEnumerable<TranscriptSegment> TranscribeAsync(IAudioSource source, CancellationToken cancellationToken);
}
