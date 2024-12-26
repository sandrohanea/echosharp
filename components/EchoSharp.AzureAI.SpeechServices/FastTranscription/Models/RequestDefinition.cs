// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Text.Json.Serialization;

namespace EchoSharp.AzureAI.SpeechServices.FastTranscription.Models;

public class RequestDefinition
{
    [JsonPropertyName("locales")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public IList<string>? Locales { get; set; }

    [JsonPropertyName("channels")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int? Channels { get; set; }

    [JsonPropertyName("diarization")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public DiarizationDefinition? Diarization { get; set; }

    [JsonPropertyName("profanityFilterMode")]
    public ProfanityFilterMode ProfanityFilterMode { get; set; }
}
