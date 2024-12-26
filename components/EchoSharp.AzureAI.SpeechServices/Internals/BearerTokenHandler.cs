// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Net;
using System.Net.Http.Headers;
using Azure.Core;

namespace EchoSharp.AzureAI.SpeechServices.Internals;

internal class BearerTokenHandler(TokenCredential tokenCredential) : HttpClientHandler
{
    private AccessToken? accessToken;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!accessToken.HasValue || accessToken.Value.ExpiresOn.Subtract(AadTokenConfig.RefreshPromptness) <= DateTimeOffset.UtcNow)
        {
            accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext([AadTokenConfig.TokenScope]), cancellationToken);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue(accessToken.Value.TokenType, accessToken.Value.Token);
        var response = await base.SendAsync(request, cancellationToken);

        // If token is revoked or expired, get a new token and retry the request
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            accessToken = await tokenCredential.GetTokenAsync(new TokenRequestContext([AadTokenConfig.TokenScope]), cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue(accessToken.Value.TokenType, accessToken.Value.Token);
            response = await base.SendAsync(request, cancellationToken);
        }

        return response;
    }
}
