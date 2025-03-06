// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.SpeechSynthesis;

/// <summary>
/// Represents a factory for creating instances of <see cref="ISpeechSynthesizer"/>.
/// </summary>
public interface ISpeechSynthesizerFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="ISpeechSynthesizer"/> with the given options.
    /// </summary>
    /// <returns></returns>
    ISpeechSynthesizer Create(SpeechSynthesizerOptions options);
}
