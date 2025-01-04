// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;

namespace EchoSharp.SpeechTranscription;

public record SpeechTranscriptorOptions
{
    public bool LanguageAutoDetect { get; set; } = true;

    public bool RetrieveTokenDetails { get; set; }

    public CultureInfo Language { get; set; } = CultureInfo.GetCultureInfo("en-us");

    public string? Prompt { get; set; }
}
