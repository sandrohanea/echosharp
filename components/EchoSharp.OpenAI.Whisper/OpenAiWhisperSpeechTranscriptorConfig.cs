// Licensed under the MIT license: https://opensource.org/licenses/MIT

using OpenAI.Audio;

namespace EchoSharp.OpenAI.Whisper;

public class OpenAiWhisperSpeechTranscriptorConfig
{
    public float? Temperature { get; set; }

    /// <summary>
    /// The API key to be used for the OpenAI client.
    /// </summary>
    /// <remarks>
    /// This value will be used only if the <see cref="AudioClient"/> is not set.
    /// If not set, the API key will be read from the environment variable "OPENAI_API_KEY".
    /// </remarks>
    public string? ApiKey { get; set; }

    /// <summary>
    /// The audio client to be used for the OpenAI client.
    /// </summary>
    /// <remarks>
    /// This value will be used instead of ApiKey if set.
    /// </remarks>
    public AudioClient? AudioClient { get; set; }

    /// <summary>
    /// A flag to run an initial warmup to create the connection.
    /// </summary>
    /// <remarks>
    /// By default, the connection is warmed up.
    /// </remarks>
    public bool WarmUp { get; set; } = true;
}
