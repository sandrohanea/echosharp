// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.SpeechProcessing;

/// <summary>
/// Represents the type of speech processing to be performed.
/// </summary>
public enum SpeechProcessingType
{
    /// <summary>
    /// Transcribes the speech to text as it is spoken, creating a transcript.
    /// </summary>
    Transcript,

    /// <summary>
    /// Translates the speech to a different language, creating a translated transcript.
    /// </summary>
    Translate,

    /// <summary>
    /// Generates a response based on the speech input and a prompt.
    /// </summary>
    PromptResponse
}