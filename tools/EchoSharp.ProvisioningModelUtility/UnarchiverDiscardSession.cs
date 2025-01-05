// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning.Unarchive;

namespace EchoSharp.ProvisioningModelUtility;

internal class UnarchiverDiscardSession : IUnarchiverSession
{
    public Task AbortAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task FlushAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
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
