// Licensed under the MIT license: https://opensource.org/licenses/MIT


// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.AzureAI.SpeechServices.SpeechSynthesis;
using EchoSharp.Provisioning;
using EchoSharp.SpeechSynthesis;
using Microsoft.CognitiveServices.Speech;

namespace EchoSharp.AzureAI.SpeechServices.RealTime;

public class AzureSpeechSynthesisProvisioner(AzureSpeechServicesConfig config, AzureSpeechSynthesizerOptions options, SpeechConfig? speechConfig = null) : ISpeechSynthesisProvisioner
{
    public async Task<ISpeechSynthesizerFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        if (speechConfig is not null)
        {
            return await new AzureSpeechSynthesizerFactory(speechConfig, options).WarmUpAsync(config.WarmUp, cancellationToken);
        }

        var endpoint = config.Endpoint;
        var subscriptionKey = config.SubscriptionKey;

        if (endpoint is not null && subscriptionKey is not null)
        {
            return await new AzureSpeechSynthesizerFactory(endpoint, subscriptionKey, options).WarmUpAsync(config.WarmUp, cancellationToken);
        }

        var azureRegion = config.AzureRegion;

        if (azureRegion is not null && subscriptionKey is not null)
        {
            return await new AzureSpeechSynthesizerFactory(azureRegion, subscriptionKey, options).WarmUpAsync(config.WarmUp, cancellationToken);
        }

        var tokenCredential = config.TokenCredential;

        if (tokenCredential is not null && azureRegion is not null && config.ResourceId is not null)
        {
            return await new AzureSpeechSynthesizerFactory(tokenCredential, config.ResourceId, azureRegion, options).WarmUpAsync(config.WarmUp, cancellationToken);
        }

        throw new InvalidOperationException("The required Azure Endpoint, Azure Region, or Azure Subscription Key for Azure AI is missing. Ensure that either the 'config.AzureRegion' or 'config.Endpoint' is provided in the configuration, or that the 'config.SubscriptionKey' or 'config.TokenCredential' is provided in the configuration.");
    }
}
