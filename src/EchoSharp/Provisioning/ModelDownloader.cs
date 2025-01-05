// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Unarchive;

namespace EchoSharp.Provisioning;

public static class ModelDownloader
{
    private const int readingBufferLength = 81920;
    private static readonly HttpClient httpClient = new();

    public static async Task DownloadModelAsync(ProvisioningModel model,
                                                IUnarchiver unarchiver,
                                                UnarchiverOptions unarchiverOptions,
                                                IHasher hasher,
                                                CancellationToken cancellationToken = default)
    {
#if NET8_0_OR_GREATER
        using var archiveStream = await httpClient.GetStreamAsync(model.Uri, cancellationToken);
#else
        using var archiveStream = await httpClient.GetStreamAsync(model.Uri);
#endif
        using var hasherSession = hasher.CreateSession();
        using var unarchiveSession = unarchiver.CreateSession(unarchiverOptions);

        // Write to the unarchiver and hash the data at the same time
        var buffer = new byte[readingBufferLength];
        int bytesRead;
#if NET8_0_OR_GREATER
        while ((bytesRead = await archiveStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            var readMemory = buffer.AsMemory(0, bytesRead);
            await unarchiveSession.PushAsync(readMemory, cancellationToken);
            await hasherSession.PushAsync(readMemory, cancellationToken);
        }
#else
        while ((bytesRead = await archiveStream.ReadAsync(buffer, 0, readingBufferLength, cancellationToken)) > 0)
        {
            await unarchiveSession.PushAsync(buffer, 0, bytesRead, cancellationToken);
            await hasherSession.PushAsync(buffer, 0, bytesRead, cancellationToken);
        }
#endif

        // Complete the hash computation
        var hash = hasherSession.GetBase64Hash();

        if (hash != model.ArchiveHash)
        {
            // First let's invalidate what we have written
            await unarchiveSession.AbortAsync(cancellationToken);
            throw new InvalidDataException("The hash of the downloaded file does not match the expected hash.");
        }

        await unarchiveSession.FlushAsync(cancellationToken);

    }
}
