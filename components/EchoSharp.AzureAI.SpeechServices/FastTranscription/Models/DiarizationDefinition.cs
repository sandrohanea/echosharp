// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Text.Json.Serialization;

namespace EchoSharp.AzureAI.SpeechServices.FastTranscription.Models;

public class DiarizationDefinition
{
    [JsonPropertyName("maxSpeakers")]
    public int MaxSpeakers { get; set; }

    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
}
