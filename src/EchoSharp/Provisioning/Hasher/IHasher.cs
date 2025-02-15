// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning.Streams;

namespace EchoSharp.Provisioning.Hasher;

public interface IHasher
{
    Task<string> ComputeHashAsync(string file, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a stream that will hash the data as it is read from the source stream.
    /// </summary>
    /// <remarks>
    /// If an expected hash is provided, the stream will verify the hash when the last data is read and will throw an exception if the hash does not match.
    /// </remarks>
    public HasherStream CreateStream(Stream source, string? expectedHash = null);

    public StringHasher CreateStringHasher();
}
