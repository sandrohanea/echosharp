// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning.Unarchive;

namespace EchoSharp.ProvisioningModelUtility;

internal class UnarchiverDiscardSession : IUnarchiverSession
{
    public Task AbortAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
    }

    public Task FlushAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public string GetIntegrityFile()
    {
        throw new NotImplementedException();
    }

    public Task PushAsync(Memory<byte> data, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
