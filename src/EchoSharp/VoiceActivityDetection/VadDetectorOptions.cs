// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.VoiceActivityDetection;

public class VadDetectorOptions
{
    public TimeSpan MinSpeechDuration { get; set; } = TimeSpan.FromMilliseconds(150);

    public TimeSpan MinSilenceDuration { get; set; } = TimeSpan.FromMilliseconds(150);
}
