// Licensed under the MIT license: https://opensource.org/licenses/MIT
namespace EchoSharp.Provisioning.Unarchive;

internal class UnarchiverNoneSession : IUnarchiverSession
{
    private readonly MemoryModel? model;
    private readonly FileStream? fileStream;
    private readonly long maxFileSize;
    private readonly Stream source;

    public UnarchiverNoneSession(Stream source, UnarchiverOptions options)
    {
        model = options.MemoryModel;
        maxFileSize = options.MaxFileSize;
        if (options.ModelPath != null)
        {
            fileStream = File.Open(Path.Combine(options.ModelPath, UnarchiverNone.ModelName), FileMode.Create, FileAccess.Write, FileShare.None);
        }

        this.source = source;
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

    public Task FlushAsync(CancellationToken cancellationToken)
    {
        if (fileStream != null)
        {
            return fileStream.FlushAsync(cancellationToken);
        }
        return Task.CompletedTask;
    }

    public string GetIntegrityFile()
    {
        throw new NotImplementedException();
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        if (fileStream != null)
        {
            // TODO: Check max size and abort if exceeded
            await source.CopyToAsync(fileStream);
            return;
        }

        await model!.CopyFromAsync(UnarchiverNone.ModelName, source, cancellationToken);
    }
}
