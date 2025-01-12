// Licensed under the MIT license: https://opensource.org/licenses/MIT

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Text;
using EchoSharp.Provisioning.Hasher;

namespace EchoSharp.Provisioning;

public class IntegrityFile
{
    private const char separator = '|';
    private const string fileSeparator = "||";

    public const string FileName = "model_integrity.txt";

    private readonly SortedDictionary<string, string> files = [];

    public void AddFile(string fileName, string hash)
    {
        // We need to make sure that fileName is consistent across all platforms
        var universalFileName = fileName.Replace('\\', '/');
        files.Add(universalFileName, hash);
    }

    public string GetIntegrityHash(IHasher hasher)
    {
        using var stringHasher = hasher.CreateStringHasher();
        return stringHasher.GetBase64Hash(GetAsString());
    }

    public IEnumerable<(string File, string Hash)> GetFiles()
    {
        foreach (var file in files)
        {
            yield return (file.Key, file.Value);
        }
    }

    public string GetAsString()
    {
        var first = true;
        var builder = new StringBuilder();
        foreach (var file in files)
        {
            if (!first)
            {
                builder.Append(fileSeparator);
            }
            else
            {
                first = false;
            }
            builder.Append(file.Key);
            builder.Append(separator);
            builder.Append(file.Value);
        }
        return builder.ToString();
    }

    public async Task WriteToFileAsync(string basePath, CancellationToken cancellationToken)
    {
        var first = true;
        using var fileStream = new FileStream(Path.Combine(basePath, FileName), FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(fileStream, Encoding.UTF8);
        foreach (var file in files)
        {
            if (!first)
            {
                await writer.WriteAsync(fileSeparator);
            }
            else
            {
                first = false;
            }
            await writer.WriteAsync(file.Key);
            await writer.WriteAsync(separator);
            await writer.WriteAsync(file.Value);
        }
#if NET8_0_OR_GREATER
        await writer.FlushAsync(cancellationToken);
#else
        await writer.FlushAsync();
#endif
    }

#if NET8_0_OR_GREATER
    [return: NotNullIfNotNull(nameof(path))]
#endif
    public static async Task<IntegrityFile?> TryReadFromFileAsync(string? path, CancellationToken cancellationToken)
    {
        if (path == null)
        {
            return null;
        }

        var fileName = Path.Combine(path, FileName);

        if (!File.Exists(fileName))
        {
            return null;
        }

        var integrityFile = new IntegrityFile();
        using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(fileStream, Encoding.UTF8);
#if NET9_0_OR_GREATER
        var text = await reader.ReadToEndAsync(cancellationToken);
        var spanText = text.AsSpan();
        foreach (var line in spanText.Split(fileSeparator))
        {
            var currentLine = spanText[line];
            var data = currentLine.Split(separator);
            if (!data.MoveNext())
            {
                return null;
            }
            var parsedFileName = currentLine[data.Current];
            if (!data.MoveNext())
            {
                return null;
            }
            var parsedHash = currentLine[data.Current];
            integrityFile.AddFile(parsedFileName.ToString(), parsedHash.ToString());
        }
#elif NET8_0_OR_GREATER
        var text = await reader.ReadToEndAsync(cancellationToken);
        foreach (var line in text.Split(fileSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            var data = line.Split(separator);
            var parsedFileName = data[0];
            if (data.Length < 2)
            {
                return null;
            }
            var parsedHash = data[1];
        }

#else
        var text = await reader.ReadToEndAsync();
        foreach (var line in text.Split([fileSeparator], StringSplitOptions.RemoveEmptyEntries))
        {
            var data = line.Split(separator);
            var parsedFileName = data[0];
            if (data.Length < 2)
            {
                return null;
            }
            var parsedHash = data[1];
        }
#endif
        return integrityFile;
    }

    public static bool operator ==(IntegrityFile file1, IntegrityFile file2)
    {
        return file1.Equals(file2);
    }

    public static bool operator !=(IntegrityFile file1, IntegrityFile file2)
    {
        return !file1.Equals(file2);
    }

    public override int GetHashCode()
    {
        return GetAsString().GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (obj is not IntegrityFile other)
        {
            return false;
        }

        if (files.Count != other.files.Count)
        {
            return false;
        }

        foreach (var file in files)
        {
            if (!other.files.TryGetValue(file.Key, out var otherHash))
            {
                return false;
            }

            if (file.Value != otherHash)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks the integrity of ALL the model files in the integrity file.
    /// </summary>
    /// <remarks>
    /// If there are other files in the directory that are not in the integrity file, they will be ignored.
    /// </remarks>
    public async Task<bool> CheckIntegrityAsync(IHasher hasher, string modelPath, CancellationToken cancellationToken)
    {
        foreach (var file in files)
        {
            var storedFileName = file.Key;      // e.g. "MySuperModel/Model.onnx" with original case
            var storedFileHash = file.Value;

            var filePathParts = storedFileName.Split(['/'], StringSplitOptions.RemoveEmptyEntries);
            var localRelativePath = Path.Combine(filePathParts);
            var fullPath = Path.Combine(modelPath, localRelativePath);
            if (!File.Exists(fullPath))
            {
                return false;
            }

            var computedHash = await hasher.ComputeHashAsync(fullPath, cancellationToken);

            if (computedHash != storedFileHash)
            {
                return false;
            }
        }
        return true;
    }
}
