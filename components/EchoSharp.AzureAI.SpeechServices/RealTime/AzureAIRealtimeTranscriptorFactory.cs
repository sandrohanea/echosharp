// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Microsoft.CognitiveServices.Speech;
using Azure.Core;
using EchoSharp.AzureAI.SpeechServices.Internals;
using EchoSharp.SpeechTranscription;

namespace EchoSharp.AzureAI.SpeechServices.RealTime;

public sealed class AzureAIRealtimeTranscriptorFactory : IRealtimeSpeechTranscriptorFactory
{
    private readonly SpeechConfig speechConfig;
    private readonly AzureAIRealtimeTranscriptorOptions options;
    private readonly RecognizerAuthTokenHandler? recognizerAuthTokenHandler;

    public AzureAIRealtimeTranscriptorFactory(string region, string subscriptionKey, AzureAIRealtimeTranscriptorOptions options)
    {
        speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
        this.options = options;
    }

    public AzureAIRealtimeTranscriptorFactory(Uri endpoint, string subscriptionKey, AzureAIRealtimeTranscriptorOptions options)
    {
        speechConfig = SpeechConfig.FromEndpoint(endpoint, subscriptionKey);
        this.options = options;
    }

    public AzureAIRealtimeTranscriptorFactory(TokenCredential tokenCredential, string resourceId, string region, AzureAIRealtimeTranscriptorOptions options)
    {
        recognizerAuthTokenHandler = new RecognizerAuthTokenHandler(tokenCredential, resourceId);
        speechConfig = SpeechConfig.FromAuthorizationToken($"aad#resourceId#token", region);
        this.options = options;
    }

    public AzureAIRealtimeTranscriptorFactory(SpeechConfig speechConfig, AzureAIRealtimeTranscriptorOptions options)
    {
        this.speechConfig = speechConfig;
        this.options = options;
    }

    public IRealtimeSpeechTranscriptor Create(RealtimeSpeechTranscriptorOptions options)
    {
        if (options.LanguageAutoDetect)
        {
            speechConfig.SetProperty(PropertyId.SpeechServiceConnection_LanguageIdMode, options.AutodetectLanguageOnce ? "AtStart" : "Continuous");
        }

        speechConfig.SetProperty(PropertyId.SpeechServiceResponse_RequestWordLevelTimestamps, options.RetrieveTokenDetails.ToString());

        return new AzureAIRealtimeTranscriptor(speechConfig, options, this.options, recognizerAuthTokenHandler);
    }

    public void Dispose()
    {

    }
}
