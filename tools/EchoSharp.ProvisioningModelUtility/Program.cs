// Licensed under the MIT license: https://opensource.org/licenses/MIT

// This utility console application is used by components maintainers to generate provisioning models for the EchoSharp library (based on URLs in the arguments).

// It downloads the archive from the URL without storing it and it computed all the hashes and sizes needed for the provisioning model.

using EchoSharp.Provisioning;
using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Streams;
using EchoSharp.ProvisioningModelUtility;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;

var httpClient = new HttpClient();
var tasks = new List<Task<ProvisioningModel>>();

var urls = args.Length > 0 ? args : GetUrls();

static string[] GetUrls()
{
    Console.WriteLine("Enter URLs to archives download:");
    var urls = new List<string>();
    string? url;
    while (!string.IsNullOrEmpty(url = Console.ReadLine()))
    {
        urls.Add(url);
    }
    return [.. urls];
}

var hasher = Sha512Hasher.Instance;

foreach (var url in urls)
{
    tasks.Add(Task.Run(async () =>
    {
        var uri = new Uri(url);

        var name = Path.GetFileNameWithoutExtension(uri.LocalPath);
        var stream = await httpClient.GetStreamAsync(uri);

        // We don't want to limit it here, we just want to know the size at the end.
        using var maxSizedStream = new MaxSizedStream(stream, long.MaxValue);
        using var archiverHasher = hasher.CreateStream(maxSizedStream, null);

        IProvisioningParser parser = Path.GetExtension(uri.LocalPath) switch
        {
            ".zip" => new ZipProvisioningParser(hasher, archiverHasher),
            ".bz2" when uri.LocalPath.EndsWith(".tar.bz2") => new TarProvisioningParser(hasher, new BZip2InputStream(new KeepOpenStream(archiverHasher))),
            ".gz" when uri.LocalPath.EndsWith(".tar.gz") => new TarProvisioningParser(hasher, new GZipInputStream(new KeepOpenStream(archiverHasher))),
            _ => new CopyProvisioningParser(hasher, archiverHasher),
        };

        await parser.RunAsync(CancellationToken.None);

        var maxFileSize = parser.GetLargestFileSize();
        var archiveHash = archiverHasher.ComputedHash;
        var archiveSize = maxSizedStream.CurrentReadSize;
        var integrityHash = parser.GetIntegrityFile().GetIntegrityHash(Sha512Hasher.Instance);
        return new ProvisioningModel(uri, parser.GetArchiveType(), archiveHash, integrityHash, archiveSize, maxFileSize);
    }));
}

await Task.WhenAll(tasks);

foreach (var task in tasks)
{
    var model = await task;
    Console.WriteLine("""
        new ProvisioningModel(
                new("{0}"),
                ProvisioningModel.ArchiveTypes.{1},
                "{2}",
                "{3}",
                {4},
                {5}),
        """,
        model.Uri, model.ArchiveType, model.ArchiveHash, model.IntegrityHash, model.ArchiveSize, model.MaxFileSize);
}
