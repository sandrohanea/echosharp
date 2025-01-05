// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning.Hasher;

public interface IHasherSession : IDisposable
{
#if NET8_0_OR_GREATER
    /// <summary>
    /// Push more archive data which will be processed by the hasher.
    /// </summary>
    /// <returns></returns>
    ValueTask PushAsync(Memory<byte> data, CancellationToken cancellationToken);
#else
    /// <summary>
    /// Push more archive data which will be processed by the hasher.
    /// </summary>
    /// <returns></returns>
    Task PushAsync(byte[] data, int offset, int count, CancellationToken cancellationToken);
#endif

    string GetBase64Hash();

}
