// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Unarchive;

namespace EchoSharp.Provisioning;

/// <summary>
/// Represents a model downloader that downloads models from the internet and verifies their integrity hash.
/// </summary>
/// <param name="httpClient"></param>
public class ModelDownloader(HttpClient httpClient)
{
    /// <summary>
    /// The default model downloader that uses 1 hour timeout for downloading models, and no authentication (models are downloaded anonymously).
    /// </summary>
    /// <remarks>
    /// If you are downloading the models from a private server or you are rate-limited by the public APIs,
    /// you should create a new instance of the ModelDownloader and set the appropriate timeout and authentication.
    /// </remarks>
    public static readonly ModelDownloader Default = new(new() { Timeout = TimeSpan.FromHours(1) });

    /// <summary>
    /// Downloads a model to a specific path and verifies the integrity hash.
    /// </summary>
    /// <remarks>
    /// This method ensures that the downloaded model is not corrupted and that the integrity hash matches the expected hash.
    /// It is recommended to use this method to download models to a specific path.
    /// It can also be used to download models in memory ensuring their integrity.
    /// If the model was previously downloaded and the integrity hash matches, the method returns without downloading the model again.
    /// </remarks>
    /// <param name="model"></param>
    /// <param name="unarchiver"></param>
    /// <param name="unarchiverOptions"></param>
    /// <param name="hasher"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task DownloadModelAsync(ProvisioningModel model,
                                                UnarchiverOptions unarchiverOptions,
                                                IHasher hasher,
                                                IUnarchiver? unarchiver = null,
                                                CancellationToken cancellationToken = default)
    {
        var integrityFile = await IntegrityFile.TryReadFromFileAsync(unarchiverOptions.ModelPath, cancellationToken);

        if (integrityFile is not null)
        {
            // We are downloading the model to a specific path
            // First we check if the model already exists and if it does and the integrity hash matches we return
            if (integrityFile.GetIntegrityHash(hasher) == model.IntegrityHash)
            {
                var integrityCheck = await integrityFile.CheckIntegrityAsync(hasher, unarchiverOptions.ModelPath!, cancellationToken);
                if (integrityCheck)
                {
                    // The model is already downloaded and the integrity hash matches
                    return;
                }
            }
        }

#if NET8_0_OR_GREATER
        using var archiveStream = await httpClient.GetStreamAsync(model.Uri, cancellationToken);
#else
        using var archiveStream = await httpClient.GetStreamAsync(model.Uri);
#endif

        unarchiver ??= model.ArchiveType switch
        {
            ProvisioningModel.ArchiveTypes.Zip => UnarchiverZip.Instance,
            ProvisioningModel.ArchiveTypes.None => UnarchiverCopy.Instance,
            _ => throw new ArgumentException($"The unarchiver must be provided for {model.ArchiveType} archive type.", nameof(unarchiver))
        };

        using var hasherStream = hasher.CreateStream(archiveStream, model.ArchiveHash);
        using var unarchiveSession = unarchiver.CreateSession(hasher, hasherStream, unarchiverOptions);

        try
        {
            await unarchiveSession.RunAsync(cancellationToken);

            if (unarchiveSession.GetIntegrityFile().GetIntegrityHash(hasher) != model.IntegrityHash)
            {
                throw new HasherException("Model integrity hash mismatch");
            }

            await unarchiveSession.FlushAsync(cancellationToken);
        }
        catch (HasherException)
        {
            await unarchiveSession.AbortAsync(cancellationToken);
            throw;
        }
    }
}
