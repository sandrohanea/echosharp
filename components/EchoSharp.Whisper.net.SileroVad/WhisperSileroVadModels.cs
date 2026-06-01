// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using Whisper.net.Ggml;

namespace EchoSharp.Whisper.net.SileroVad;

/// <summary>
/// This is a provisioning model that can be obtained from <seealso cref="WhisperSileroVadModels"/>.
/// </summary>
public class WhisperSileroVadModel(Uri uri, ProvisioningModel.ArchiveTypes archiveType, string archiveHash, string integrityHash, long archiveSize, long maxFileSize) : ProvisioningModel(uri, archiveType, archiveHash, integrityHash, archiveSize, maxFileSize);

/// <summary>
/// Collection of provisioning models for Whisper.net ggml Silero VAD.
/// </summary>
public static class WhisperSileroVadModels
{
    public static readonly WhisperSileroVadModel V5_1_2 = new(
        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v4/vad/ggml-silero-v5.1.2.bin"),
        ProvisioningModel.ArchiveTypes.None,
        "IXTzHURxZYBUs4CyHwbz0PzNu/bBEHliQZhOT+J/uPz6PtDiI4h0gvR+K9lVyfATGB3OrncMbKk6hCK9MmoGiQ==",
        "0/fRgzAfIW/F6JtIPEzZT8dV7hUUhl64WxUlYS8Hc6S6QqTHqOsVSTBuL4sar4hOST9j1GDHydA3Bf2wAXDy9w==",
        885098,
        885098);

    public static readonly WhisperSileroVadModel V6_2_0 = new(
        new("https://huggingface.co/sandrohanea/whisper.net/resolve/v4/vad/ggml-silero-v6.2.0.bin"),
        ProvisioningModel.ArchiveTypes.None,
        "tsqSILGnsVtJJcCc6eKyjYJw8qYU4vmhWnUsDKmzTBZ7wPvKKqymhu3AMH0j85hEImTdOeIO/JcZnprEb9QnFg==",
        "YP3Ggnx64CkaW4Tt98TU3hjgUJ0Eo2chKtCX1gszqkjxpBek0vTuHMMnXfKn95xplwEHu4yo2gk0aS7GkzTbFw==",
        885098,
        885098);

    public static WhisperSileroVadModel GetModel(SileroVadType type)
    {
        return type switch
        {
            SileroVadType.V5_1_2 => V5_1_2,
            SileroVadType.V6_2_0 => V6_2_0,
            _ => throw new NotSupportedException($"Silero VAD model type {type} is not supported.")
        };
    }
}
