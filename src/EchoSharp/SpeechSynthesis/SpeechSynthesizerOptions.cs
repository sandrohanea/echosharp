// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;

namespace EchoSharp.SpeechSynthesis;

public class SpeechSynthesizerOptions
{
    public string DefaultVoice { get; set; } = string.Empty;

    public CultureInfo? DefaultLanguage { get; set; }
}
