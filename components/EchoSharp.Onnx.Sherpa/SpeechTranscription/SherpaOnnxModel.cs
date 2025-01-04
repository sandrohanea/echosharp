// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SherpaOnnx;
using static EchoSharp.Onnx.Sherpa.SpeechTranscription.SherpaOnnxModel;

namespace EchoSharp.Onnx.Sherpa.SpeechTranscription;

public class SherpaOnnxModels
{
    public static SherpaOnnxModel ZipFormerGigaSpeech = new SherpaOnnxOfflineModel(
        "sherpa-onnx-zipformer-gigaspeech-2023-12-12",
        "PBRjpvAXW6a9qOMDb+YpeYG7+cMTJBiaahiCrNJN5k+4UuRKLlHxmPsTMYdpv9285u4HQ19SKKDYp8k9b3aeIA==",
        (string path, ref OfflineModelConfig config) =>
    {
        config.Transducer.Encoder = Path.Combine(path, "encoder-epoch-30-avg-1.onnx");
        config.Transducer.Decoder = Path.Combine(path, "decoder-epoch-30-avg-1.onnx");
        config.Transducer.Joiner = Path.Combine(path, "joiner-epoch-30-avg-1.onnx");
        config.Tokens = Path.Combine(path, "tokens.txt");
    });

    public static SherpaOnnxModel ZipFormerGigaSpeechInt8 = new SherpaOnnxOfflineModel(
        "sherpa-onnx-zipformer-gigaspeech-2023-12-12",
        "PBRjpvAXW6a9qOMDb+YpeYG7+cMTJBiaahiCrNJN5k+4UuRKLlHxmPsTMYdpv9285u4HQ19SKKDYp8k9b3aeIA==",
        (string path, ref OfflineModelConfig config) =>
    {
        config.Transducer.Encoder = Path.Combine(path, "encoder-epoch-30-avg-1.int8.onnx");
        config.Transducer.Decoder = Path.Combine(path, "decoder-epoch-30-avg-1.int8.onnx");
        config.Transducer.Joiner = Path.Combine(path, "joiner-epoch-30-avg-1.int8.onnx");
        config.Tokens = Path.Combine(path, "tokens.txt");
    });

    public static SherpaOnnxModel StreamingZipformerCtcMultiZhHans = new SherpaOnnxOnlineModel(
        "sherpa-onnx-streaming-zipformer-ctc-multi-zh-hans-2023-12-13",
        "POfp6ScsjzXU6wssFKn8kgW0Vj1BuYDPePM/XerDmTRIMtBMRHfdmnNeuJ9WRD6uf33+HUvMkaZc9M/xOqIQgA==",
        (string path, ref OnlineModelConfig config) =>
    {
        config.Zipformer2Ctc.Model = Path.Combine(path, "ctc-epoch-20-avg-1-chunk-16-left-128.onnx");
        config.Tokens = Path.Combine(path, "tokens.txt");
    });


    public static SherpaOnnxModel StreamingZipformerCtcMultiZhHansInt8 = new SherpaOnnxOnlineModel(
        "sherpa-onnx-streaming-zipformer-ctc-multi-zh-hans-2023-12-13",
        "POfp6ScsjzXU6wssFKn8kgW0Vj1BuYDPePM/XerDmTRIMtBMRHfdmnNeuJ9WRD6uf33+HUvMkaZc9M/xOqIQgA==",
        (string path, ref OnlineModelConfig config) =>
    {
        config.Zipformer2Ctc.Model = Path.Combine(path, "ctc-epoch-20-avg-1-chunk-16-left-128.int8.onnx");
        config.Tokens = Path.Combine(path, "tokens.txt");
    });

    public static SherpaOnnxModel WhisperDistilSmallEn = new SherpaOnnxOfflineModel("sherpa-onnx-whisper-distil-small.en",
        "TWddmQPUls58bUmJDdGmg/QJb2FmpNHMIjmreNEv2dZbEhOX3RKe9AOWVYxeAzTJ7jxPsSsZGXwgPUJzRp/I0g==",
        (string path, ref OfflineModelConfig config) =>
    {
        config.Whisper.Encoder = Path.Combine(path, "encoder-epoch-30-avg-1.onnx");
        config.Whisper.Decoder = Path.Combine(path, "decoder-epoch-30-avg-1.onnx");
        config.Tokens = Path.Combine(path, "distil-small.en-tokens.txt");
    });

    public static SherpaOnnxModel WhisperDistilSmallEnInt8 = new SherpaOnnxOfflineModel(
        "sherpa-onnx-whisper-distil-small.en",
        "TWddmQPUls58bUmJDdGmg/QJb2FmpNHMIjmreNEv2dZbEhOX3RKe9AOWVYxeAzTJ7jxPsSsZGXwgPUJzRp/I0g==",
        (string path, ref OfflineModelConfig config) =>
    {
        config.Whisper.Encoder = Path.Combine(path, "encoder-epoch-30-avg-1.int8.onnx");
        config.Whisper.Decoder = Path.Combine(path, "decoder-epoch-30-avg-1.int8.onnx");
        config.Tokens = Path.Combine(path, "distil-small.en-tokens.txt");
    });

    // TODO: Add more Sherpa Models
}

public class SherpaOnnxModel(string name, string hash)
{
    public string Name { get; } = name;
    public string Hash { get; } = hash;
}

public delegate void SherpaOfflineAction(string path, ref OfflineModelConfig config);
public delegate void SherpaOnlineAction(string path, ref OnlineModelConfig config);

public class SherpaOnnxOfflineModel(string name, string hash, SherpaOfflineAction load) : SherpaOnnxModel(name, hash)
{
    public SherpaOfflineAction Load { get; } = load;
}

public class SherpaOnnxOnlineModel(string name, string hash, SherpaOnlineAction load) : SherpaOnnxModel(name, hash)
{
    public SherpaOnlineAction Load { get; } = load;
}
