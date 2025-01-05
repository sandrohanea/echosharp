// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning.Unarchive;

internal class UnarchiverNoneSession : IUnarchiverSession
{
    private readonly MemoryModel? model;
    private readonly FileStream? fileStream;
    private readonly long maxFileSize;
    private long writtenSize;

    public UnarchiverNoneSession(UnarchiverOptions options)
    {
        model = options.MemoryModel;
        maxFileSize = options.MaxFileSize;
        if (options.ModelPath != null)
        {
            fileStream = File.Open(Path.Combine(options.ModelPath, UnarchiverNone.ModelName), FileMode.Create, FileAccess.Write, FileShare.None);
        }
    }

    public async Task AbortAsync(CancellationToken cancellationToken)
    {
        if (fileStream != null)
        {
            fileStream.SetLength(0);
            await fileStream.FlushAsync(cancellationToken);
            return;
        }
        model!.DeleteModel(UnarchiverNone.ModelName);
    }

    public void Dispose()
    {
        fileStream?.Dispose();
    }

#if NET8_0_OR_GREATER
    public async Task PushAsync(Memory<byte> data, CancellationToken cancellationToken)
    {
        if (writtenSize + data.Length > maxFileSize)
        {
            throw new UnarchiveException($"File size exceeds the limit of {maxFileSize}");
        }

        if (fileStream != null)
        {
            await fileStream.WriteAsync(data, cancellationToken);
        }
        else
        {
            await model!.AppendAsync(UnarchiverNone.ModelName, data, cancellationToken);
        }
        writtenSize += data.Length;
    }
#else
    public async Task PushAsync(byte[] data, int offset, int count, CancellationToken cancellationToken)
    {
        if (writtenSize + data.Length > maxFileSize)
        {
            throw new UnarchiveException($"File size exceeds the limit of {maxFileSize}");
        }

        if (fileStream != null)
        {
            await fileStream.WriteAsync(data, offset, count, cancellationToken);
        }
        else
        {
            await model!.AppendAsync(UnarchiverNone.ModelName, data, offset, count, cancellationToken);
        }
        writtenSize += data.Length;
    }
#endif

    public Task FlushAsync(CancellationToken cancellationToken)
    {
        if (fileStream != null)
        {
            return fileStream.FlushAsync(cancellationToken);
        }
        return Task.CompletedTask;
    }
}
