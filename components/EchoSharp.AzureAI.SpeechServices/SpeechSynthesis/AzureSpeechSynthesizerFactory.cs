// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Azure.Core;
using EchoSharp.AzureAI.SpeechServices.Internals;
using EchoSharp.SpeechSynthesis;
using Microsoft.CognitiveServices.Speech;

namespace EchoSharp.AzureAI.SpeechServices.SpeechSynthesis;

public class AzureSpeechSynthesizerFactory : ISpeechSynthesizerFactory
{
    private readonly SpeechConfig speechConfig;
    private readonly AzureSpeechSynthesizerOptions options;
    private readonly AuthTokenHandler? authTokenHandler;

    public AzureSpeechSynthesizerFactory(string region, string subscriptionKey, AzureSpeechSynthesizerOptions options)
    {
        speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
        this.options = options;
    }

    public AzureSpeechSynthesizerFactory(Uri endpoint, string subscriptionKey, AzureSpeechSynthesizerOptions options)
    {
        speechConfig = SpeechConfig.FromEndpoint(endpoint, subscriptionKey);
        this.options = options;
    }

    public AzureSpeechSynthesizerFactory(TokenCredential tokenCredential, string resourceId, string region, AzureSpeechSynthesizerOptions options)
    {
        authTokenHandler = new AuthTokenHandler(tokenCredential, resourceId);
        speechConfig = SpeechConfig.FromAuthorizationToken($"aad#resourceId#token", region);
        this.options = options;
    }

    public AzureSpeechSynthesizerFactory(SpeechConfig speechConfig, AzureSpeechSynthesizerOptions options)
    {
        this.speechConfig = speechConfig;
        this.options = options;
    }

    public ISpeechSynthesizer Create(SpeechSynthesizerOptions options)
    {
        if (options.DefaultLanguage != null)
        {
            speechConfig.SpeechSynthesisLanguage = options.DefaultLanguage.Name;
        }

        return new AzureSpeechSynthesizer(speechConfig, options, this.options, authTokenHandler);
    }

    public void Dispose()
    {
    }
}
