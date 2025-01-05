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

#if NET8_0_OR_GREATER
    public async Task AppendAsync(string modelPath, Memory<byte> memory, CancellationToken cancellationToken)
    {
        if (!memoryFiles.TryGetValue(modelPath, out var memoryFile))
        {
            memoryFile = new MemoryStream();
            memoryFiles[modelPath] = memoryFile;
        }
        await memoryFile.WriteAsync(memory, cancellationToken);
    }
#else

    public async Task AppendAsync(string modelPath, byte[] data, int offset, int count, CancellationToken cancellationToken)
    {
        if (!memoryFiles.TryGetValue(modelPath, out var memoryFile))
        {
            memoryFile = new MemoryStream();
            memoryFiles[modelPath] = memoryFile;
        }
        await memoryFile.WriteAsync(data, offset, count, cancellationToken);
    }
#endif
}
