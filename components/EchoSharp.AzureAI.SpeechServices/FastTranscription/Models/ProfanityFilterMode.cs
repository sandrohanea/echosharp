// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Text.Json.Serialization;

namespace EchoSharp.AzureAI.SpeechServices.FastTranscription.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProfanityFilterMode
{
    /// <summary>
    /// Profanity filtering is disabled.
    /// </summary>
    None,
    /// <summary>
    /// Profanity filtering is enabled.
    /// </summary>
    Masked,
    /// <summary>
    /// The profanity is removed.
    /// </summary>
    Removed,

    /// <summary>
    /// The profanity is replaced with tags.
    /// </summary>
    Tags
}
