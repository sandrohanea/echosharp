// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Azure.Core;
using Microsoft.CognitiveServices.Speech;

namespace EchoSharp.AzureAI.SpeechServices.Internals;

internal class AuthTokenHandler(TokenCredential tokenCredential, string resourceId) : IDisposable
{
    private bool isInitialized;
    private string? currentAccessToken;
    private readonly CancellationTokenSource refreshTokenCancellation = new();

#if NET9_0_OR_GREATER
    private readonly Lock syncRoot = new();
#else
    private readonly object syncRoot = new();
#endif

    private readonly List<AuthTokenLoader> loaders = [];

    /// <summary>
    /// Initializes the token handler by retrieving the access token and starting the token refresh process
    /// </summary>
    /// <remarks>
    /// This needs to be called before any loader is retrieved.
    /// </remarks>
    /// <returns></returns>
    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        lock (syncRoot)
        {
            if (isInitialized)
            {
                return Task.CompletedTask;
            }

            isInitialized = true;
            return InitializeAsyncInternal(cancellationToken);
        }
    }

    public void Dispose()
    {
        refreshTokenCancellation.Cancel();
    }

    public AuthTokenLoader GetLoader(PropertyCollection properties)
    {
        if(currentAccessToken is null)
        {
            throw new InvalidOperationException("The token handler has not been initialized.");
        }
        
        lock (syncRoot)
        {
            var loader = new AuthTokenLoader(properties, this);
            loaders.Add(loader);
            loader.LoadToken(currentAccessToken);
            return loader;
        }
    }

    internal void Remove(AuthTokenLoader loader)
    {
        lock (syncRoot)
        {
            loaders.Remove(loader);
        }
    }

    private static ValueTask<AccessToken> GetTokenAsync(TokenCredential tokenCredential, CancellationToken cancellationToken)
    {
        return tokenCredential.GetTokenAsync(new TokenRequestContext([AadTokenConfig.TokenScope]), cancellationToken);
    }

    private void RefreshTokenForAllLoaders(AccessToken newToken)
    {
        lock (syncRoot)
        {
            currentAccessToken = FormatAccessToken(newToken, resourceId);
            foreach (var loader in loaders)
            {
                loader.LoadToken(currentAccessToken);
            }
        }
    }

    private static string FormatAccessToken(AccessToken token, string resourceId)
    {
        return $"aad#{resourceId}#{token.Token}";
    }

    private Task RefreshTokenAsync(DateTimeOffset initialTokenExpiry, CancellationToken token)
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
                    RefreshTokenForAllLoaders(newToken);
                    nextRefresh = newToken.ExpiresOn - AadTokenConfig.RefreshPromptness;
                }
                catch (Exception)
                {
                    // TODO: Log the exception somehow once EchoSharp has a logging mechanism
                    // Ignore token refresh errors
                }
            }
        }, token);
    }

    private async Task InitializeAsyncInternal(CancellationToken cancellationToken)
    {
        var initialToken = await GetTokenAsync(tokenCredential, cancellationToken);
        currentAccessToken = FormatAccessToken(initialToken, resourceId);

        // Intentional fire and forget for refreshing the tokens
        _ = RefreshTokenAsync(initialToken.ExpiresOn, refreshTokenCancellation.Token);
    }
}
