// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using EchoSharp.SpeechProcessing;

namespace EchoSharp.AzureAI.SpeechServices.FastTranscription;

public class AzureAIFastTranscriptorProvisioner(AzureSpeechServicesConfig config) : ISpeechProcessorProvisioner
{
    public async Task<ISpeechProcessorFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        var endpoint = config.Endpoint is not null
            ? config.Endpoint
            : config.AzureRegion is not null
                ? new Uri($"https://{config.AzureRegion}.api.cognitive.microsoft.com")
                : null;

        if (endpoint is null)
        {
            throw new InvalidOperationException("The required Azure Endpoint for Azure AI is missing. Ensure that either the 'config.AzureRegion' or 'config.Endpoint' is provided in the configuration.");
        }

        if (config.TokenCredential is null && string.IsNullOrWhiteSpace(config.SubscriptionKey))
        {
            throw new InvalidOperationException("The required Azure Subscription Key or Token Credential for Azure AI is missing. Ensure that either the 'config.SubscriptionKey' or 'config.TokenCredential' is provided in the configuration.");
        }

        var factory = config.TokenCredential is not null
            ? new AzureAIFastTranscriptorFactory(endpoint, config.TokenCredential)
            : new AzureAIFastTranscriptorFactory(endpoint, config.SubscriptionKey!);

        return await factory.WarmUpAsync(config.WarmUp, cancellationToken);
    }

}
