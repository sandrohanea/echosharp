// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.SpeechSynthesis;

public struct SpeechSegment(string text)
{
    public string Text { get; set; } = text;

    public string? OverrideVoice { get; set; }
}
