// Licensed under the MIT license: https://opensource.org/licenses/MIT

using EchoSharp.Provisioning;
using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Streams;
using EchoSharp.Provisioning.Unarchive;

namespace EchoSharp.ProvisioningModelUtility;

public class CopyProvisioningParser : ProvisioningParserBase
{
    private readonly IHasher hasher;
    private readonly Stream source;
    private long largestFileSize;

    public CopyProvisioningParser(IHasher hasher, Stream source) : base(hasher, source)
    {
        this.hasher = hasher;
        this.source = source;
    }

    public override ProvisioningModel.ArchiveTypes GetArchiveType()
    {
        return ProvisioningModel.ArchiveTypes.None;
    }

    public override long GetLargestFileSize()
    {
        return largestFileSize;
    }

    public override async Task RunAsync(CancellationToken cancellationToken)
    {
        using var maxSizedSource = new MaxSizedStream(source, long.MaxValue);
        using var hasherStream = hasher.CreateStream(maxSizedSource, null);
        await hasherStream.CopyToAsync(Stream.Null, cancellationToken);

        GetIntegrityFile().AddFile(UnarchiverCopy.ModelName, hasherStream.ComputedHash);

        largestFileSize = maxSizedSource.CurrentReadSize;
    }

    protected override IAsyncEnumerable<UnarchiveFileEntry> EnumerateEntriesAsync(Stream stream, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
