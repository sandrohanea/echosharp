// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.IO.Compression;
using EchoSharp.Provisioning;
using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Unarchive;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace EchoSharp.ProvisioningModelUtility;

internal class UnarchiverDiscardHelper(IHasher hasher, Stream source)
{
    private readonly IntegrityFile integrityFile = new();

    private long largestFileSize;
    private ArchiveType? archiveType;

    public IntegrityFile GetIntegrityFile()
    {
        // Return the computed integrity file, even though no data was written.
        return integrityFile;
    }

    public long GetLargestFileSize()
    {
        return largestFileSize;
    }

    public ArchiveType? GetArchiveType()
    {
        return archiveType;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Use SharpCompress to open the archive

            using var zipArchive = new ZipArchive(source, ZipArchiveMode.Read, leaveOpen: false);

            archiveType = ArchiveType.Zip;

            foreach (var entry in zipArchive.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Skip directories
                if (string.IsNullOrEmpty(entry.Name))
                {
                    continue;
                }


                // Open the entry stream
                using var entryStream = entry.Open();

                // Create a hash stream for computing the file's hash
                using var hasherStream = hasher.CreateStream(entryStream, null);

                await ConsumeStreamAsync(hasherStream, cancellationToken);

                // Store the hash in the integrity file
                if (hasherStream.ComputedHash != null)
                {
                    integrityFile.AddFile(entry.FullName, hasherStream.ComputedHash);
                }
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
            if (hasherStream.ComputedHash != null)
            {
                integrityFile.AddFile(UnarchiverCopy.ModelName, hasherStream.ComputedHash);
            }
        }
    }

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
