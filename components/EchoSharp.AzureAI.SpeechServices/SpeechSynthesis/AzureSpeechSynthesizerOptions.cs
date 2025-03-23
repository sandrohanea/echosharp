// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.AzureAI.SpeechServices.SpeechSynthesis;

public class AzureSpeechSynthesizerOptions
{
    /// <summary>
    /// Gets or sets the speaking rate (speed) of the voice.
    /// Value range: 0.5 to 2.0. Default is 1.0.
    /// </summary>
    public double SpeakingRate { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the pitch of the voice.
    /// Value range: 0.5 to 2.0. Default is 1.0.
    /// </summary>
    public double Pitch { get; set; } = 1.0;

    /// <summary>
    /// Gets or sets the volume of the voice.
    /// Value range: 0 to 100. Default is 100.
    /// </summary>
    public int Volume { get; set; } = 100;
}
