// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using EchoSharp.Abstractions.Audio;
using EchoSharp.Abstractions.SpeechTranscription;
using EchoSharp.Audio;
using EchoSharp.AzureAI.SpeechServices.FastTranscription.Models;

namespace EchoSharp.AzureAI.SpeechServices.FastTranscription;

internal sealed class AzureAIFastTranscriptor(HttpClient httpClient, SpeechTranscriptorOptions options) : ISpeechTranscriptor
{
    public void Dispose()
    {

    }

    public async IAsyncEnumerable<TranscriptSegment> TranscribeAsync(IAudioSource source, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/speechtotext/transcriptions:transcribe?api-version=2024-11-15");

        List<string>? languages = options.LanguageAutoDetect ? null : [options.Language.ToString()];

        var definition = new RequestDefinition()
        {
            Locales = languages
        };

        var multipartContent = new MultipartFormDataContent
        {
            { new StreamContent(new WaveFileStream(source)), "audio", "audio.wav" },
            { new StringContent(JsonSerializer.Serialize(definition)), "definition" }
        };
        httpRequest.Content = multipartContent;

        using var responseMessage = await httpClient.SendAsync(httpRequest, cancellationToken);

        // Handle the error caused by unprocessable entity
        if ((int)responseMessage.StatusCode == 422 || (int)responseMessage.StatusCode == 400)
        {
#if NET8_0_OR_GREATER
            using var errorStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken: cancellationToken);
#else
            using var errorStream = await responseMessage.Content.ReadAsStreamAsync();
#endif
            var errorResponse = await JsonSerializer.DeserializeAsync<ErrorResponse>(errorStream, cancellationToken: cancellationToken);

            throw new TranscriptGenerationException(errorResponse ?? new ErrorResponse() { Code = "CannotDeserializeError", Message = "Cannot deserialize error from the response." });
        }
        responseMessage.EnsureSuccessStatusCode();

#if NET8_0_OR_GREATER
        using var responseStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken: cancellationToken);
#else
        using var responseStream = await responseMessage.Content.ReadAsStreamAsync();
#endif

        var response = await JsonSerializer.DeserializeAsync<TranscriptResponse>(responseStream, cancellationToken: cancellationToken);

        foreach (var phrase in response?.Phrases ?? [])
        {
            yield return new TranscriptSegment()
            {
                ConfidenceLevel = phrase.Confidence,
                Language = phrase.Locale != null ? new CultureInfo(phrase.Locale) : null,
                StartTime = TimeSpan.FromMilliseconds(phrase.OffsetMilliseconds),
                Duration = TimeSpan.FromMilliseconds(phrase.DurationMilliseconds),
                Text = phrase.Text,
                Tokens = phrase.Words?
                    .Select(word => new TranscriptToken()
                    {
                        Text = word.Text,
                        StartTime = TimeSpan.FromMilliseconds(word.OffsetMilliseconds),
                        Duration = TimeSpan.FromMilliseconds(word.DurationMilliseconds),
                    })?
                    .ToList()
            };
        }
    }
}
