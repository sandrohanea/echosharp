// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.SpeechProcessing;

/// <summary>
/// Factory for creating instances of <see cref="ISpeechProcessorFactory"/>.
/// </summary>
public interface ISpeechProcessorFactory : IDisposable
{
    /// <summary>
    /// Creates a new instance of <see cref="ISpeechProcessor"/>.
    /// </summary>
    /// <returns></returns>
    ISpeechProcessor Create(SpeechProcessorOptions options);
}
