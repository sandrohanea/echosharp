// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Azure.Core;

namespace EchoSharp.AzureAI.SpeechServices.FastTranscription;

public class AzureAIFastTranscriptorConfig
{
    /// <summary>
    /// The subscription key to be used for the Azure AI client.
    /// </summary>
    /// <remarks>
    /// This value will be used only if the <see cref="TokenCredential"/> is not set.
    /// If not set, the Subscription key will be read from the environment variable "AZURE_AI_SUBSCRIPTION_KEY".
    /// </remarks>
    public string? SubscriptionKey { get; set; }

    /// <summary>
    /// The TokenCredential to be used for the Azure AI client.
    /// </summary>
    /// <remarks>
    /// This value will be used instead of ApiKey if set.
    /// </remarks>
    public TokenCredential? TokenCredential { get; set; }

    /// <summary>
    /// The Azure region to be used for the Azure AI client.
    /// </summary>
    /// <remarks>
    /// This value will be used only if the <see cref="Endpoint"/> is not set.
    /// If not set, the Azure region will be read from the environment variable "AZURE_AI_REGION".
    /// </remarks>
    public string? AzureRegion { get; set; }

    /// <summary>
    /// The endpoint to be used for the Azure AI client.
    /// </summary>
    /// <remarks>
    /// This value will be used instead of the AzureRegion if set.
    /// </remarks>
    public Uri? Endpoint { get; set; }

    /// <summary>
    /// A flag to run an initial warmup to create the connection.
    /// </summary>
    /// <remarks>
    /// By default, the connection is warmed up.
    /// </remarks>
    public bool WarmUp { get; set; } = true;
}
