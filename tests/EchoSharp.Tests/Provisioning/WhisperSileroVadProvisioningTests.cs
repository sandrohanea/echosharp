// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Unarchive;
using EchoSharp.Whisper.net.SileroVad;
using Whisper.net.Ggml;
using Xunit;

namespace EchoSharp.Tests.Provisioning;

public class WhisperSileroVadProvisioningTests
{
    [Theory]
    [InlineData(SileroVadType.V5_1_2)]
    [InlineData(SileroVadType.V6_2_0)]
    public async Task UnarchiverCopy_CopiesGgmlSileroVadModelSizeWithExactMaxFileSize(SileroVadType modelType)
    {
        var model = WhisperSileroVadModels.GetModel(modelType);
        var sourceBytes = new byte[model.ArchiveSize];
        using var source = new MemoryStream(sourceBytes);
        var outputPath = Path.Combine(AppContext.BaseDirectory, $"whisper-silero-vad-{Guid.NewGuid():N}");

        Directory.CreateDirectory(outputPath);
        try
        {
            using var session = UnarchiverCopy.Instance.CreateSession(
                Sha512Hasher.Instance,
                source,
                new(outputPath, model.MaxFileSize));

            await session.RunAsync(CancellationToken.None);

            var copiedModelPath = Path.Combine(outputPath, UnarchiverCopy.ModelName);
            Assert.Equal(sourceBytes.Length, new FileInfo(copiedModelPath).Length);
        }
        finally
        {
            Directory.Delete(outputPath, recursive: true);
        }
    }
}
