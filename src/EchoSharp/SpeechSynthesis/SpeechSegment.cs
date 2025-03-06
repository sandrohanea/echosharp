// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.SpeechSynthesis;

public class SpeechSegment
{
    public string Text { get; set; } = string.Empty;

    public string? OverrideVoice { get; set; }
}
