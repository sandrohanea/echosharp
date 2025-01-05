// Licensed under the MIT license: https://opensource.org/licenses/MIT

// This utility console application is used to generate provisioning models for the EchoSharp library (based on URLs in the arguments).

using System.Security.Cryptography;
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
    while ((url = Console.ReadLine()) != null)
    {
        urls.Add(url);
    }
    return [.. urls];
}

var unarchiver = new UnarchiverDiscard();

foreach (var url in urls)
{
    tasks.Add(Task.Run(async () =>
    {
        var uri = new Uri(url);
        var session = unarchiver.CreateSession(new UnarchiverOptions());
        var name = Path.GetFileNameWithoutExtension(uri.LocalPath);
        var stream = await httpClient.GetStreamAsync(uri);
        var sha512 = SHA512.Create();
        sha512.Initialize();
        var buffer = new byte[81920];
        int bytesRead;
        var archiver = ArchiveFactory.Open(stream);
        while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
        {
            sha512.TransformBlock(buffer, 0, bytesRead, buffer, 0);
            await session.PushAsync(buffer.AsMemory(0, bytesRead), CancellationToken.None);
        }
        sha512.TransformFinalBlock(buffer, 0, 0);
        var archiveHash = Convert.ToBase64String(sha512.Hash!);
        var archiveSize = stream.Length;
        await session.FlushAsync(CancellationToken.None);

    }));

}

await Task.WhenAll(tasks);

