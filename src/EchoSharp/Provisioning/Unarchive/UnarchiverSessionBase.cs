// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Streams;

namespace EchoSharp.Provisioning.Unarchive;

public abstract class UnarchiverSessionBase(IHasher hasher, Stream source, UnarchiverOptions options) : IUnarchiverSession
{
    private readonly IntegrityFile integrityFile = new();

    ~UnarchiverSessionBase()
    {
        Dispose(disposing: false);
    }

    public virtual Task FlushAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns the IntegrityFile containing the list of extracted files 
    /// and their computed hashes.
    /// </summary>
    public IntegrityFile GetIntegrityFile()
    {
        return integrityFile;
    }

    /// <summary>
    /// Aborts the unarchiving process, removing any partially‑written files.
    /// </summary>
    public async Task AbortAsync(CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(options.ModelPath))
        {
            // Attempt to remove each file from disk that we recorded in integrityFile.
            foreach (var fileRecord in integrityFile.GetFiles())
            {
                var extractedPath = Path.Combine(options.ModelPath, fileRecord.File);
                if (File.Exists(extractedPath))
                {
                    File.Delete(extractedPath);
                }
            }
        }
        else if (options.MemoryModel != null)
        {
            // Remove each file from in-memory model
            foreach (var fileRecord in integrityFile.GetFiles())
            {
                options.MemoryModel.DeleteModel(fileRecord.File);
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Reads the ZIP data from <see cref="source"/> and unzips it to disk 
    /// or an in-memory model. Each file is hashed and added to <see cref="integrityFile"/>.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        // Leave the stream open if you need to do something afterward, but generally
        // you can close once the archive is processed.
        var rootPath = !string.IsNullOrEmpty(options.ModelPath)
            ? Path.GetFullPath(options.ModelPath) + Path.DirectorySeparatorChar
            : null;

        await foreach (var entry in EnumerateEntriesAsync(source, cancellationToken))
        {
            // Skip directories
            if (entry.IsDirectory)
            {
                continue;
            }

            // Open the entry’s stream
            using var entryStream = await entry.OpenReadAsync(cancellationToken);

            // Limit the size of the entry stream
            using var maxSizedEntryStream = new MaxSizedStream(entryStream, options.MaxFileSize);

            // Create a hasher for each file so we can get its computed hash (no need to enforce expected hash here, as the archive is checked instead)
            using var hasherEntryStream = hasher.CreateStream(maxSizedEntryStream);

            if (!string.IsNullOrEmpty(options.ModelPath))
            {
                // Extract to disk
                var destinationPath = Path.GetFullPath(Path.Combine(options.ModelPath, entry.FullName));

                // Ensure the destination is inside our rootPath
                if (!destinationPath.StartsWith(rootPath!, StringComparison.OrdinalIgnoreCase))
                {
                    throw new IOException(
                        $"Entry '{entry.FullName}' is trying to extract outside of the target directory."
                    );
                }

                // Ensure directory structure exists
                var destDirectory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destDirectory) && !Directory.Exists(destDirectory))
                {
                    Directory.CreateDirectory(destDirectory);
                }
                using var fileStream = File.Open(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
#if NET8_0_OR_GREATER
                await hasherEntryStream.CopyToAsync(fileStream, cancellationToken);
#else
                await hasherEntryStream.CopyToAsync(fileStream);
#endif
            }
            else if (options.MemoryModel != null)
            {
                // Extract to memory model
                await options.MemoryModel.CopyFromAsync(
                    entry.FullName,
                    hasherEntryStream,
                    cancellationToken);
            }
            else
            {
                // If no ModelPath or MemoryModel is provided, we have nowhere to write. 
                // Decide how you want to handle that—throw or skip.
                throw new InvalidOperationException(
                    "No output specified (ModelPath and MemoryModel are both null).");
            }

            // Record the file in the IntegrityFile
            integrityFile.AddFile(entry.FullName, hasherEntryStream.ComputedHash);
        }

        // Finally, if we’re extracting to disk and want to persist the IntegrityFile there:
        if (!string.IsNullOrEmpty(options.ModelPath))
        {
            await integrityFile.WriteToFileAsync(options.ModelPath!, cancellationToken);
        }
    }

    protected abstract IAsyncEnumerable<UnarchiveFileEntry> EnumerateEntriesAsync(Stream stream, CancellationToken cancellationToken);

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Dispose of managed resources
        }
        // Dispose of unmanaged resources
    }

}
