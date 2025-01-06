// Licensed under the MIT license: https://opensource.org/licenses/MIT

// This utility console application is used to generate provisioning models for the EchoSharp library (based on URLs in the arguments).

using System.Security.Cryptography;
using EchoSharp.Provisioning.Hasher;
using EchoSharp.Provisioning.Unarchive;
using EchoSharp.ProvisioningModelUtility;
using SharpCompress.Archives;

var httpClient = new HttpClient();
var tasks = new List<Task>();

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

foreach (var url in urls)
{
    tasks.Add(Task.Run(async () =>
    {
        var uri = new Uri(url);

        var name = Path.GetFileNameWithoutExtension(uri.LocalPath);
        var stream = await httpClient.GetStreamAsync(uri);
        var archiverHasher = Sha512Hasher.Instance.CreateStream(stream, null);
        var unarchiver = new UnarchiverDiscardHelper(Sha512Hasher.Instance, stream);
        await unarchiver.RunAsync(CancellationToken.None);

        var maxFileSize = unarchiver.GetLargestFileSize();
        var archiveHash = archiverHasher.ComputedHash;
        var archiveSize = stream.Length;
        var integrityHash = unarchiver.GetIntegrityFile().GetIntegrityHash(Sha512Hasher.Instance);

        Console.WriteLine($"new ProvisioningModel(\"{name}\", \"{uri}\", \"{archiveHash}\", \"{integrityHash}\", {archiveSize}, {maxFileSize}),");

    }));

}

await Task.WhenAll(tasks);

