
// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.AzureAI.SpeechServices.Internals;

public sealed class OptionalDisposable(IDisposable? target) : IDisposable
{
    public void Dispose()
    {
        if (target is not null)
        {
            target.Dispose();
        }
    }
}
