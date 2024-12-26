// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.AzureAI.SpeechServices.FastTranscription.Models;

namespace EchoSharp.AzureAI.SpeechServices.FastTranscription;

public class TranscriptGenerationException(ErrorResponse error) : Exception(error.Message)
{
    public ErrorResponse Error { get; } = error;
}
