// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Unarchive;

namespace EchoSharp.ProvisioningModelUtility;

public abstract class ProvisioningParserBase(IHasher hasher, Stream source) : IProvisioningParser
{
    private readonly IntegrityFile integrityFile = new();

    private long largestFileSize;

    public IntegrityFile GetIntegrityFile()
    {
        // Return the computed integrity file, even though no data was written.
        return integrityFile;
    }

    public virtual long GetLargestFileSize()
    {
        return largestFileSize;
    }

    public abstract ProvisioningModel.ArchiveTypes GetArchiveType();

    public virtual async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var entry in EnumerateEntriesAsync(source, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Skip directories
                if (entry.IsDirectory)
                {
                    continue;
                }

                // Open the entry stream
                using var entryStream = await entry.OpenReadAsync(cancellationToken);

                // Create a hash stream for computing the file's hash
                using var hasherStream = hasher.CreateStream(entryStream, null);

                await ConsumeStreamAsync(hasherStream, cancellationToken);

                // Store the hash in the integrity file
                integrityFile.AddFile(entry.FullName, hasherStream.ComputedHash);
            }
        }
        catch (InvalidOperationException ex)
        {
            if (source.Position > 0)
            {
                Console.WriteLine("The archive could not be opened: " + ex.Message);
                Console.WriteLine("The archive was partially read. Cannot copy the source.");
                throw;
            }

            // Create a hash stream for computing the file's hash
            using var hasherStream = hasher.CreateStream(source, null);

            await ConsumeStreamAsync(hasherStream, cancellationToken);

            // Store the hash in the integrity file
            integrityFile.AddFile(UnarchiverCopy.ModelName, hasherStream.ComputedHash);
        }
    }

    protected abstract IAsyncEnumerable<UnarchiveFileEntry> EnumerateEntriesAsync(Stream stream, CancellationToken cancellationToken);

    private async Task ConsumeStreamAsync(Stream stream, CancellationToken cancellationToken)
    {
        long fileSize = 0;
        var buffer = new byte[8192];
        int bytesRead;
        while ((bytesRead = await stream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            fileSize += bytesRead; // Accumulate the number of bytes read
        }

        // Track the largest file based on real size
        if (fileSize > largestFileSize)
        {
            largestFileSize = fileSize;
        }
    }
}

