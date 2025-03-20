// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Azure.Core;
using Microsoft.CognitiveServices.Speech;

namespace EchoSharp.AzureAI.SpeechServices.Internals;

internal class AuthTokenLoader(PropertyCollection properties, AuthTokenHandler handler) : IDisposable
{
    public void Dispose()
    {
        handler.Remove(this);
    }

    public void LoadToken(string accessToken)
    {
        properties.SetProperty(PropertyId.SpeechServiceAuthorization_Token, accessToken);
    }
}
