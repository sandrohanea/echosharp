// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;

namespace EchoSharp.SpeechProcessing;

/// <summary>
/// Options for configuring speech transcription.
/// </summary>
public record SpeechProcessorOptions
{
    /// <summary>
    /// The type of speech processing to be performed. Defaults to Transcript.
    /// </summary>
    public SpeechProcessingType Type { get; set; } = SpeechProcessingType.Transcript;

    /// <summary>
    /// Indicates whether automatic language detection is enabled. Defaults to true.
    /// </summary>
    public bool LanguageAutoDetect { get; set; } = true;

    /// <summary>
    /// Indicates whether to retrieve token details. It is a boolean property that can be set to true or false.
    /// </summary>
    public bool RetrieveTokenDetails { get; set; }

    /// <summary>
    /// Represents the language setting, defaulting to English (United States). It allows for localization based on the
    /// specified culture.
    /// </summary>
    public CultureInfo Language { get; set; } = CultureInfo.GetCultureInfo("en-us");

    /// <summary>
    /// Represents an optional prompt string that can be set or retrieved. It allows for nullable string values.
    /// </summary>
    public string? Prompt { get; set; }

    /// <summary>
    /// Represents the target language setting for translation and response creation. See <see cref="SpeechProcessingType"/>.
    /// </summary>
    public CultureInfo? TargetLanguage { get; set; }
}
