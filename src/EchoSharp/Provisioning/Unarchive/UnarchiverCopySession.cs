// Licensed under the MIT license: https://opensource.org/licenses/MIT
using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Streams;

namespace EchoSharp.Provisioning.Unarchive;

internal class UnarchiverCopySession(IHasher hasher, Stream source, UnarchiverOptions options) : IUnarchiverSession
{
    private readonly IntegrityFile integrityFile = new();
    private readonly FileStream? fileStream = options.ModelPath is not null
        ? File.Open(Path.Combine(options.ModelPath, UnarchiverCopy.ModelName), FileMode.Create, FileAccess.Write, FileShare.None)
        : null;

    public async Task AbortAsync(CancellationToken cancellationToken)
    {
        if (fileStream != null)
        {
            fileStream.SetLength(0);
            await fileStream.FlushAsync(cancellationToken);
            return;
        }
        options.MemoryModel!.DeleteModel(UnarchiverCopy.ModelName);
    }

    public void Dispose()
    {
        fileStream?.Dispose();
    }

    public Task FlushAsync(CancellationToken cancellationToken)
    {
        if (fileStream != null)
        {
            return fileStream.FlushAsync(cancellationToken);
        }
        return Task.CompletedTask;
    }

    public IntegrityFile GetIntegrityFile()
    {
        return integrityFile;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using var maxSizedSource = new MaxSizedStream(source, options.MaxFileSize);
        using var hasherStream = hasher.CreateStream(maxSizedSource, null);
        if (fileStream != null)
        {
            // TODO: Check max size and abort if exceeded
#if NET8_0_OR_GREATER
            await hasherStream.CopyToAsync(fileStream, cancellationToken);
#else
            await hasherStream.CopyToAsync(fileStream);
#endif
        }
        else
        {
            await options.MemoryModel!.CopyFromAsync(UnarchiverCopy.ModelName, maxSizedSource, cancellationToken);
        }

        integrityFile.AddFile(UnarchiverCopy.ModelName, hasherStream.ComputedHash);

        if (options.ModelPath != null)
        {
            await integrityFile.WriteToFileAsync(options.ModelPath, cancellationToken);
        }
    }
}
