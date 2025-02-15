// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using SherpaOnnx;

namespace EchoSharp.Onnx.Sherpa.SpeechTranscription;

public class SherpaOnnxModel(
    string name,
    Uri uri,
    ProvisioningModel.ArchiveTypes archiveType,
    string archiveHash,
    string integrityHash,
    long archiveSize,
    long maxFileSize)
    : ProvisioningModel(uri, archiveType, archiveHash, integrityHash, archiveSize, maxFileSize)
{
    public string Name { get; } = name;
}

public delegate void SherpaOfflineAction(string path, ref OfflineModelConfig config);
public delegate void SherpaOnlineAction(string path, ref OnlineModelConfig config);

public class SherpaOnnxOfflineModel(string name,
    Uri uri,
    ProvisioningModel.ArchiveTypes archiveType,
    string archiveHash,
    string integrityHash,
    long archiveSize,
    long maxFileSize,
    SherpaOfflineAction load) : SherpaOnnxModel(name, uri, archiveType, archiveHash, integrityHash, archiveSize, maxFileSize)
{
    public SherpaOfflineAction Load { get; } = load;
}

public class SherpaOnnxOnlineModel(
    string name,
    Uri uri,
    ProvisioningModel.ArchiveTypes archiveType,
    string archiveHash,
    string integrityHash,
    long archiveSize,
    long maxFileSize,
    SherpaOnlineAction load) : SherpaOnnxModel(name, uri, archiveType, archiveHash, integrityHash, archiveSize, maxFileSize)
{
    public SherpaOnlineAction Load { get; } = load;
}
