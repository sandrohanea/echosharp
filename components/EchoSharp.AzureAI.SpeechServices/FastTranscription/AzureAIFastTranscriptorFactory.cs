// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Azure.Core;
using EchoSharp.Abstractions.SpeechTranscription;
using EchoSharp.AzureAI.SpeechServices.Internals;

namespace EchoSharp.AzureAI.SpeechServices.FastTranscription;

public sealed class AzureAIFastTranscriptorFactory : ISpeechTranscriptorFactory
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

    public ISpeechTranscriptor Create(SpeechTranscriptorOptions options)
    {
        return new AzureAIFastTranscriptor(httpClient, options);
    }

    public void Dispose()
    {

    }
}
