// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Text.Json.Serialization;

namespace EchoSharp.AzureAI.SpeechServices.FastTranscription.Models;

public class ErrorResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("innerError")]
    public InnerError? InnerError { get; set; }
}

public class InnerError
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

