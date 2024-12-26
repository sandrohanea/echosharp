// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Azure.Core;
using Microsoft.CognitiveServices.Speech;

namespace EchoSharp.AzureAI.SpeechServices.Internals;

internal class RecognizerTokenLoader(SpeechRecognizer speechRecognizer) : IDisposable
{
    public SpeechRecognizer SpeechRecognizer => speechRecognizer;
    private CancellationTokenSource? loadTokenCancellation;
    public void Dispose()
    {
        loadTokenCancellation?.Dispose();
    }

    public async Task LoadTokenAsync(TokenCredential tokenCredential, string resourceId, CancellationToken cancellationToken)
    {
        var initialToken = await GetTokenAsync(tokenCredential, cancellationToken);
        speechRecognizer.AuthorizationToken = FormatAccessToken(initialToken, resourceId);
        loadTokenCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _ = RefreshTokenAsync(tokenCredential, resourceId, initialToken.ExpiresOn, loadTokenCancellation.Token);
    }

    private Task RefreshTokenAsync(TokenCredential tokenCredential, string resourceId, DateTimeOffset initialTokenExpiry, CancellationToken token)
    {
        return Task.Run(async () =>
        {
            var nextRefresh = initialTokenExpiry - AadTokenConfig.RefreshPromptness;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(nextRefresh - DateTimeOffset.UtcNow, token);
                    var newToken = await GetTokenAsync(tokenCredential, token);
                    speechRecognizer.AuthorizationToken = FormatAccessToken(newToken, resourceId);
                    nextRefresh = newToken.ExpiresOn - AadTokenConfig.RefreshPromptness;
                }
                catch (Exception)
                {
                    // Ignore token refresh errors
                }
            }
        }, token);
    }

    private static ValueTask<AccessToken> GetTokenAsync(TokenCredential tokenCredential, CancellationToken cancellationToken)
    {
        return tokenCredential.GetTokenAsync(new TokenRequestContext([AadTokenConfig.TokenScope]), cancellationToken);
    }

    private static string FormatAccessToken(AccessToken token, string resourceId)
    {
        return $"aad#{resourceId}#{token.Token}";
    }
}
