// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Security.Cryptography;
using EchoSharp.Audio;
using EchoSharp.SpeechTranscription;

namespace EchoSharp.Provisioning;

public static class ProvisioningUtils
{
    private const int defaultBufferCopyLength = 81920;
    private static readonly SHA512 sha512 = SHA512.Create();

    public static async Task<string> GetSha512HashAsync(this string path, CancellationToken cancellationToken)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
#if NET8_0_OR_GREATER
        var hashBytes = await sha512.ComputeHashAsync(stream);
#else
        var hashBytes = sha512.ComputeHash(stream);
        await Task.CompletedTask;
#endif
        return Convert.ToBase64String(hashBytes);
    }

    public static async Task<ISpeechTranscriptorFactory> WarmUpAsync(this ISpeechTranscriptorFactory factory, bool warmUp, CancellationToken cancellationToken)
    {
        if (warmUp)
        {
            var options = new SpeechTranscriptorOptions()
            {
                LanguageAutoDetect = false,
                RetrieveTokenDetails = true,
            };
            using var warmUpTranscritor = factory.Create(options);
            var transcribeEvents = warmUpTranscritor.TranscribeAsync(new SilenceAudioSource(TimeSpan.FromSeconds(1), 16000), cancellationToken);
            await foreach (var _ in transcribeEvents.WithCancellation(cancellationToken))
            {
                // Do nothing
            }
        }
        return factory;
    }

    public static async Task SaveToFileAsync(this Stream stream, string path, CancellationToken cancellationToken)
    {
        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
        await stream.CopyToAsync(fileStream, defaultBufferCopyLength, cancellationToken);
    }
}
