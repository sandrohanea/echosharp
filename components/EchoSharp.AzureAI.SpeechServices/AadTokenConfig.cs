// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.AzureAI.SpeechServices;

public class AadTokenConfig
{
    /// <summary>
    /// The scope of the token to request (default is https://cognitiveservices.azure.com/.default)
    /// </summary>
    public static string TokenScope { get; set; } = "https://cognitiveservices.azure.com/.default";

    // The time before the token expires to request a new token (default is 3 minutes)
    public static TimeSpan RefreshPromptness { get; set; } = TimeSpan.FromMinutes(3);
}
