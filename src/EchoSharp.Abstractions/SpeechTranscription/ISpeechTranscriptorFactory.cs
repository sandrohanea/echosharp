// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Abstractions.SpeechTranscription;

/// <summary>
/// Factory for creating instances of <see cref="ISpeechTranscriptorFactory"/>.
/// </summary>
public interface ISpeechTranscriptorFactory : IDisposable
{
    /// <summary>
    /// Creates a new instance of <see cref="ISpeechTranscriptor"/>.
    /// </summary>
    /// <returns></returns>
    ISpeechTranscriptor Create(SpeechTranscriptorOptions options);
}
