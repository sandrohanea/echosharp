// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning;

public sealed class MemoryModel : IDisposable
{
    private readonly Dictionary<string, MemoryStream> memoryFiles = [];

    public void Dispose()
    {
        foreach (var memoryFile in memoryFiles.Values)
        {
            memoryFile.Dispose();
        }
    }

    public byte[] GetModel(string modelPath)
    {
        return memoryFiles[modelPath].ToArray();
    }

    public void DeleteModel(string modelPath)
    {
        if (memoryFiles.TryGetValue(modelPath, out var value))
        {
            value.Dispose();
            memoryFiles.Remove(modelPath);
        }
    }

    public Memory<byte> GetModelMemory(string modelPath)
    {
        var memoryFile = memoryFiles[modelPath];
        return memoryFile.GetBuffer().AsMemory(0, (int)memoryFile.Length);
    }

    public async Task CopyFromAsync(string modelPath, Stream source, CancellationToken cancellationToken)
    {
        if (!memoryFiles.TryGetValue(modelPath, out var memoryFile))
        {
            memoryFile = new MemoryStream();
            memoryFiles[modelPath] = memoryFile;
        }
#if NET8_0_OR_GREATER
        await source.CopyToAsync(memoryFile, cancellationToken);
#else
        await source.CopyToAsync(memoryFile);
#endif
    }

}
