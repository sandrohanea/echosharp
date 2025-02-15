// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net;
using Whisper.net.Ggml;

namespace EchoSharp.Whisper.net;

public class WhisperSpeechTranscriptorConfig
{
    /// <summary>
    /// The path where the model file will be downloaded and used for transcription.
    /// </summary>
    /// <remarks>
    /// If this is set, the model file will be downloaded and used for transcription.
    /// If this is not set, the model will be downloaded in memory and used for transcription (not persisted to disk).
    /// Defaults to <see langword="null"/>.
    /// This should be either an existing directory or a directory that can be created.
    /// </remarks>
    public string? ModelPath { get; set; }

    /// <summary>
    /// The model size to be used.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="GgmlType.Base"/>.
    /// </remarks>
    public GgmlType GgmlType { get; set; } = GgmlType.Base;

    /// <summary>
    /// The quantization type to be used.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="QuantizationType.NoQuantization"/>.
    /// </remarks>
    public QuantizationType QuantizationType { get; set; } = QuantizationType.NoQuantization;

    /// <summary>
    /// The options to be used for the Whisper factory.
    /// </summary>
    /// <remarks>
    /// Defaults to a new instance of <see cref="WhisperFactoryOptions"/>, which has default values as per the Whisper.net library.
    /// </remarks>
    public WhisperFactoryOptions WhisperFactoryOptions { get; set; } = new WhisperFactoryOptions();

    /// <summary>
    /// The path to the CoreML encoder model to be downloaded and used for transcription.
    /// </summary>
    /// <remarks>
    /// If this is set, the CoreML encoder model file will be downloaded.
    /// If this is not set, the CoreML encoder model will not be downloaded.
    /// By default, the CoreML encoder model is not downloaded.
    /// Note: The modelc directory needs to be extracted at the same level as the "ggml-base.bin" file (and the current executable).
    /// </remarks>
    public string? CoreMLEncoderModelPath { get; set; }

    /// <summary>
    /// The path to the OpenVino encoder model to be downloaded and used for transcription.
    /// </summary>
    /// <remarks>
    /// If this is set, the OpenVino encoder model file will be downloaded.
    /// If this is not set, the OpenVino encoder model will not be downloaded.
    /// By default, the OpenVino encoder model is not downloaded.
    /// </remarks>
    public string? OpenVinoEncoderModelPath { get; set; }

    /// <summary>
    /// A flag to run an initial warmup of the model.
    /// </summary>
    /// <remarks>
    /// For some runtimes, it is beneficial to run an initial warmup of the model as initial inference can be slower.
    /// By default, the model is warmed up.
    /// </remarks>
    public bool WarmUp { get; set; } = true;
}
