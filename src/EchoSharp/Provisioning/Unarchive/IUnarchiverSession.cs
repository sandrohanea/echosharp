// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning.Unarchive;

public interface IUnarchiverSession : IDisposable
{
    Task RunAsync(CancellationToken cancellationToken);

    Task FlushAsync(CancellationToken cancellationToken);
    Task AbortAsync(CancellationToken cancellationToken);
    IntegrityFile GetIntegrityFile();
}
