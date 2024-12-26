// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;
using System.Runtime.CompilerServices;
using OpenAI.Audio;
using EchoSharp.Abstractions.Audio;
using EchoSharp.Abstractions.SpeechTranscription;
using EchoSharp.Audio;

namespace EchoSharp.OpenAI.Whisper;

public sealed class OpenAIWhisperSpeechTranscriptor(AudioClient audioClient, AudioTranscriptionOptions options) : ISpeechTranscriptor
{
    // Only the extension is important for the openai server to validate the content.
    private const string fileName = "audio.wav";

    public void Dispose()
    {
    }

    public async IAsyncEnumerable<TranscriptSegment> TranscribeAsync(IAudioSource source, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var waveStream = new WaveFileStream(source);

        var result = await audioClient.TranscribeAudioAsync(waveStream, fileName, options, cancellationToken);

        if (result.Value is not null)
        {
            var language = new CultureInfo(result.Value.Language);
            foreach (var segment in result.Value.Segments)
            {
                var tokens = new List<TranscriptToken>(segment.TokenIds.Length);
                for (var i = 0; i < segment.TokenIds.Length; i++)
                {
                    tokens.Add(new TranscriptToken()
                    {
                        Id = segment.TokenIds.Span[i],
                    });
                }

                yield return new TranscriptSegment()
                {
                    StartTime = segment.StartTime,
                    Duration = segment.EndTime - segment.StartTime,
                    Language = language,
                    Text = segment.Text,
                    ConfidenceLevel = segment.AverageLogProbability,
                    Tokens = tokens,
                };
            }
        }
    }
}
