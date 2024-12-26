// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Text.Json.Serialization;

namespace EchoSharp.AzureAI.SpeechServices.FastTranscription.Models;

public class TranscriptResponse
{
    [JsonPropertyName("durationMilliseconds")]
    public long DurationMilliseconds { get; set; }

    [JsonPropertyName("combinedPhrases")]
    public IList<CombinePharaseDetails>? CombinedPhrases { get; set; }

    [JsonPropertyName("phrases")]
    public IList<Phrase>? Phrases { get; set; }
}

public class CombinePharaseDetails
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class Phrase
{
    [JsonPropertyName("offsetMilliseconds")]
    public long OffsetMilliseconds { get; set; }

    [JsonPropertyName("durationMilliseconds")]
    public long DurationMilliseconds { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("words")]
    public IList<Word>? Words { get; set; }

    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    [JsonPropertyName("confidence")]
    public float Confidence { get; set; }
}

public class Word
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("offsetMilliseconds")]
    public long OffsetMilliseconds { get; set; }

    [JsonPropertyName("durationMilliseconds")]
    public long DurationMilliseconds { get; set; }
}

