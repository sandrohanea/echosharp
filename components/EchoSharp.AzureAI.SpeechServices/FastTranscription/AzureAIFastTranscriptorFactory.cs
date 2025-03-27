// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Azure.Core;
using EchoSharp.AzureAI.SpeechServices.Internals;
using EchoSharp.SpeechProcessing;

namespace EchoSharp.AzureAI.SpeechServices.FastTranscription;

public sealed class AzureAIFastTranscriptorFactory : ISpeechProcessorFactory
{
    private readonly HttpClient httpClient;

    public AzureAIFastTranscriptorFactory(string region, string subscriptionKey)
    {
        httpClient = new HttpClient()
        {
            BaseAddress = new Uri($"https://{region}.api.cognitive.microsoft.com")
        };
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
    }

    public AzureAIFastTranscriptorFactory(Uri endpoint, string subscriptionKey)
    {
        httpClient = new HttpClient()
        {
            BaseAddress = endpoint
        };
        httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
    }

    public AzureAIFastTranscriptorFactory(Uri endpoint, TokenCredential tokenCredential)
    {
        httpClient = new HttpClient(new BearerTokenHandler(tokenCredential))
        {
            BaseAddress = endpoint
        };
    }

    public AzureAIFastTranscriptorFactory(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public ISpeechProcessor Create(SpeechProcessorOptions options)
    {
        if (options.Type != SpeechProcessingType.Transcript)
        {
            throw new NotSupportedException("Only Transcript processing is supported by AzureAIFastTranscriptor");
        }

        return new AzureAIFastTranscriptor(httpClient, options);
    }

    public void Dispose()
    {

    }
}
