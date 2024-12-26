// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Globalization;

namespace EchoSharp.AzureAI.SpeechServices.RealTime;

public class AzureAIRealtimeTranscriptorOptions
{
    private static readonly IList<CultureInfo> defaultCandidateLanguages = [new CultureInfo("en-US"), new CultureInfo("es-ES"), new CultureInfo("de-DE"), new CultureInfo("zh-CN")];
    /// <summary>
    /// Azure Speech Services requires a list of candidate languages to be provided if automatic language detection is enabled.
    /// </summary>
    /// <remarks>
    /// The default list of candidate languages is English, Spanish, German, and Chinese.
    /// More details here: https://learn.microsoft.com/en-us/azure/ai-services/speech-service/language-identification?tabs=once&pivots=programming-language-csharp
    /// Also, if the language identification is enabled only at the start of the conversation, the list of candidate languages should be at most 4.
    /// For continuous language identification, the list of candidate languages should be at most 10.
    /// </remarks>
    public IList<CultureInfo> CandidateLanguages { get; set; } = defaultCandidateLanguages;
}
