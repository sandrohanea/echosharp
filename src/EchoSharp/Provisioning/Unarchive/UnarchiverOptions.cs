// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace EchoSharp.Provisioning.Unarchive;

public struct UnarchiverOptions
{
    public UnarchiverOptions(string modelPath, long maxFileSize)
    {
        ModelPath = modelPath;
        MaxFileSize = maxFileSize;
    }

    public UnarchiverOptions(MemoryModel memoryModel, long maxFileSize)
    {
        MemoryModel = memoryModel;
        MaxFileSize = maxFileSize;
    }

    public string? ModelPath { get; set; }

    public MemoryModel? MemoryModel { get; set; }

    public long MaxFileSize { get; set; }
}
