// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using SherpaOnnx;

namespace EchoSharp.Onnx.Sherpa.SpeechTranscription;

public class SherpaOnnxModels
{
    public static readonly SherpaOnnxModel ZipFormerGigaSpeech = new SherpaOnnxOfflineModel(
        "sherpa-onnx-zipformer-gigaspeech-2023-12-12",
        new("https://github.com/k2-fsa/sherpa-onnx/releases/download/asr-models/sherpa-onnx-zipformer-gigaspeech-2023-12-12.tar.bz2"),
        ProvisioningModel.ArchiveTypes.TarGz,
        "G3nYkxNWEGRbNsyoeh/5coPspVE0S4Ted3da+xgmc4w1rW6gH3sxGO/YHCSZ8mZi3WOHjF9RgrgJD0regyZPvA==",
        "4Y9U5HTgtUgx3IaPFSycEB+biwokWPAAeaYikqffZTQ92k8Bvma2ux9ddWXT7H6GtHrdrZGOGKzMd/c51IY6Iw==",
        307011274L,
        260990607L,
        (string path, ref OfflineModelConfig config) =>
    {
        config.Transducer.Encoder = Path.Combine(path, "encoder-epoch-30-avg-1.onnx");
        config.Transducer.Decoder = Path.Combine(path, "decoder-epoch-30-avg-1.onnx");
        config.Transducer.Joiner = Path.Combine(path, "joiner-epoch-30-avg-1.onnx");
        config.Tokens = Path.Combine(path, "tokens.txt");
    });

    public static readonly SherpaOnnxModel ZipFormerGigaSpeechInt8 = new SherpaOnnxOfflineModel(
        "sherpa-onnx-zipformer-gigaspeech-2023-12-12",
        new("https://github.com/k2-fsa/sherpa-onnx/releases/download/asr-models/sherpa-onnx-zipformer-gigaspeech-2023-12-12.tar.bz2"),
        ProvisioningModel.ArchiveTypes.TarGz,
        "G3nYkxNWEGRbNsyoeh/5coPspVE0S4Ted3da+xgmc4w1rW6gH3sxGO/YHCSZ8mZi3WOHjF9RgrgJD0regyZPvA==",
        "4Y9U5HTgtUgx3IaPFSycEB+biwokWPAAeaYikqffZTQ92k8Bvma2ux9ddWXT7H6GtHrdrZGOGKzMd/c51IY6Iw==",
        307011274L,
        260990607L,
        (string path, ref OfflineModelConfig config) =>
    {
        config.Transducer.Encoder = Path.Combine(path, "encoder-epoch-30-avg-1.int8.onnx");
        config.Transducer.Decoder = Path.Combine(path, "decoder-epoch-30-avg-1.int8.onnx");
        config.Transducer.Joiner = Path.Combine(path, "joiner-epoch-30-avg-1.int8.onnx");
        config.Tokens = Path.Combine(path, "tokens.txt");
    });

    // TODO: Add more Sherpa Models
}

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
