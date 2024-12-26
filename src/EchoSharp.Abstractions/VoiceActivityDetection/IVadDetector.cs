// Licensed under the MIT license: https://opensource.org/licenses/MIT
using EchoSharp.Abstractions.Audio;

namespace EchoSharp.Abstractions.VoiceActivityDetection;

/// <summary>
/// Represents a voice activity detection component that can detect voice activity segments in mono-channel audio samples at 16 kHz.
/// </summary>
public interface IVadDetector
{
    /// <summary>
    /// Detects voice activity segments in the given audio source.
    /// </summary>
    /// <param name="source">The audio source to analyze.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    IAsyncEnumerable<VadSegment> DetectSegmentsAsync(IAudioSource source, CancellationToken cancellationToken);
}
