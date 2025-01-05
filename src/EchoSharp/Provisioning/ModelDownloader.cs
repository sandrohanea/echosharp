// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Unarchive;

namespace EchoSharp.Provisioning;

public static class ModelDownloader
{
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
        using var hasherStream = hasher.CreateStream(archiveStream);
        using var unarchiveSession = unarchiver.CreateSession(hasherStream, unarchiverOptions);

        await unarchiveSession.RunAsync(cancellationToken);

        if (hasherStream.GetBase64Hash() != model.ArchiveHash)
        {
            await unarchiveSession.AbortAsync(cancellationToken);
            throw new InvalidDataException("The hash of the downloaded file does not match the expected hash.");
        }

        await unarchiveSession.FlushAsync(cancellationToken);

    }
}
