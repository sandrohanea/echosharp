// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Collections.Concurrent;
using System.Security.Cryptography;
using EchoSharp.Audio;
using EchoSharp.SpeechTranscription;
using EchoSharp.VoiceActivityDetection;

namespace EchoSharp.Provisioning;

public static class ProvisioningUtils
{
    private static ConcurrentBag<SHA256> sha256Pool = new();

    private const int defaultBufferCopyLength = 81920;

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

    public static async Task<IVadDetectorFactory> WarmUpAsync(this IVadDetectorFactory factory, bool warmUp, CancellationToken cancellationToken)
    {
        if (warmUp)
        {
            var options = new VadDetectorOptions();
            using var warmUpDetector = factory.CreateVadDetector(options);
            var vadEvents = warmUpDetector.DetectSegmentsAsync(new SilenceAudioSource(TimeSpan.FromSeconds(1), 16000), cancellationToken);
            await foreach (var _ in vadEvents.WithCancellation(cancellationToken))
            {
                // Do nothing
            }
        }
        return factory;
    }

    public static async Task SaveToFileAsync(this Stream stream, string path, string expectedHash, int maxSize = 10, CancellationToken cancellationToken = default)
    {
        var sha256 = sha256Pool.TryTake(out var sha256Instance) ? sha256Instance : SHA256.Create();
        try
        {
            sha256.Initialize();
            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            using var hashingStream = new CryptoStream(Stream.Null, sha256, CryptoStreamMode.Write);
            // Write to the temporary file and compute the hash simultaneously
            var buffer = new byte[81920];
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                await hashingStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            }

            // Complete the hash computation
            hashingStream.FlushFinalBlock();

            var hash = BitConverter.ToString(sha256.Hash).Replace("-", string.Empty);

            if (!string.Equals(hash, expectedHash, StringComparison.OrdinalIgnoreCase))
            {
                // First let's invalidate what we have written
                fileStream.SetLength(0);
                throw new InvalidDataException("The hash of the downloaded file does not match the expected hash.");
            }

            await fileStream.FlushAsync(cancellationToken);

        }
        finally
        {
            sha256Pool.Add(sha256);
        }
    }
}
