// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EchoSharp.Provisioning;
using EchoSharp.SpeechTranscription;

namespace EchoSharp.AzureAI.SpeechServices.FastTranscription;

public class AzureAIFastTranscriptorProvisioner(AzureAIFastTranscriptorConfig config) : ISpeechTranscriptorProvisioner
{
    private const string azureAIRegionEnvVariable = "AZURE_AI_REGION";
    private const string azureAISubscriptionKeyEnvVariable = "AZURE_AI_SUBSCRIPTION_KEY";

    public async Task<ISpeechTranscriptorFactory> ProvisionAsync(CancellationToken cancellationToken = default)
    {
        var endpoint = config.Endpoint is not null
            ? config.Endpoint
            : config.AzureRegion is not null
                ? new Uri($"https://{config.AzureRegion}.api.cognitive.microsoft.com")
                : GetEndpointFromEnvVariable();

        var factory = config.TokenCredential is not null
            ? new AzureAIFastTranscriptorFactory(endpoint, config.TokenCredential)
            : new AzureAIFastTranscriptorFactory(endpoint, GetAzureSubscriptionKeyFromEnvVariable());

        return await factory.WarmUpAsync(config.WarmUp, cancellationToken);
    }

    private static Uri GetEndpointFromEnvVariable()
    {
        var envVariable = Environment.GetEnvironmentVariable(azureAIRegionEnvVariable)
            ?? throw new InvalidOperationException($"The required Azure Endpoint for Azure AI is missing. Ensure that either the environment variable '{azureAIRegionEnvVariable}' is set or that 'config.AzureRegion' or 'config.Endpoint' is provided in the configuration.");
        return new Uri($"https://{envVariable}.api.cognitive.microsoft.com");
    }

    private static string GetAzureSubscriptionKeyFromEnvVariable()
    {
        var envVariable = Environment.GetEnvironmentVariable(azureAISubscriptionKeyEnvVariable)
            ?? throw new InvalidOperationException("The required Azure Subscription Key for Azure AI is missing. Ensure that the environment variable '{azureAISubscriptionKeyEnvVariable}' is set or that 'config.SubscriptionKey' or 'config.TokenCredential' is provided in the configuration.");
        return envVariable;
    }
}
