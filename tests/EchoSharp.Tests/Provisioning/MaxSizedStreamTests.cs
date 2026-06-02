// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning.Streams;
using Xunit;

namespace EchoSharp.Tests.Provisioning;

public class MaxSizedStreamTests
{
    [Fact]
    public async Task CopyToAsync_WhenSourceLengthMatchesMaxSize_AllowsDefaultReadAttemptPastEnd()
    {
        var sourceBytes = new byte[MaxSizedStream.DefaultReadAttemptTolerance];
        using var source = new MemoryStream(sourceBytes);
        using var maxSizedStream = new MaxSizedStream(source, sourceBytes.Length);
        using var destination = new MemoryStream();

        await maxSizedStream.CopyToAsync(destination);

        Assert.Equal(sourceBytes.Length, destination.Length);
    }

    [Fact]
    public async Task CopyToAsync_WhenSourceExceedsMaxSize_ThrowsBeforeCopyingExtraBytes()
    {
        var maxSize = MaxSizedStream.DefaultReadAttemptTolerance;
        var sourceBytes = new byte[maxSize + 1];
        using var source = new MemoryStream(sourceBytes);
        using var maxSizedStream = new MaxSizedStream(source, maxSize);
        using var destination = new MemoryStream();

        await Assert.ThrowsAsync<InvalidOperationException>(() => maxSizedStream.CopyToAsync(destination));
        Assert.Equal(maxSize, destination.Length);
    }

    [Fact]
    public async Task CopyToAsync_WhenReadAttemptToleranceIsZero_ThrowsOnFixedSizeReadPastMaxSize()
    {
        var sourceBytes = new byte[1];
        using var source = new MemoryStream(sourceBytes);
        using var maxSizedStream = new MaxSizedStream(source, sourceBytes.Length, readAttemptTolerance: 0);
        using var destination = new MemoryStream();

        await Assert.ThrowsAsync<InvalidOperationException>(() => maxSizedStream.CopyToAsync(destination));
    }
}
