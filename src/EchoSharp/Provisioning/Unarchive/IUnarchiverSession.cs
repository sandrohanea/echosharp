// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning.Unarchive;

public interface IUnarchiverSession : IDisposable
{

#if NET8_0_OR_GREATER
    /// <summary>
    /// Push more archive data which will be processed by the unarchiver.
    /// </summary>
    /// <returns></returns>
    Task PushAsync(Memory<byte> data, CancellationToken cancellationToken);
#else
    /// <summary>
    /// Push more archive data which will be processed by the unarchiver.
    /// </summary>
    /// <returns></returns>
    public Task PushAsync(byte[] data, int offset, int count, CancellationToken cancellationToken);
#endif

    Task AbortAsync(CancellationToken cancellationToken);

    Task FlushAsync(CancellationToken cancellationToken);

    string GetIntegrityFile();
}
