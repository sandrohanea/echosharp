// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Azure.Core;
using Microsoft.CognitiveServices.Speech;

namespace EchoSharp.AzureAI.SpeechServices.Internals;

internal class RecognizerAuthTokenHandler(TokenCredential tokenCredential, string resourceId)
{
    public async Task<RecognizerTokenLoader> GetLoaderAsync(SpeechRecognizer recognizer, CancellationToken cancellationToken)
    {
        var loader = new RecognizerTokenLoader(recognizer);
        await loader.LoadTokenAsync(tokenCredential, resourceId, cancellationToken);
        return loader;
    }
}
