# EchoSharp.SharpZipLib

**EchoSharp.SharpZipLib** is a utility component that provides compression and decompression functionality for EchoSharp using the [SharpZipLib library](https://github.com/icsharpcode/SharpZipLib).

## Overview

This component enables EchoSharp's model provisioning system to work with various compressed archive formats. It implements the `IUnarchiver` interface, allowing it to be used with EchoSharp's `ModelDownloader` for extracting compressed models and resources.

## Key Features

- **Multiple Archive Format Support**: Handle ZIP, TAR.GZ, TAR.BZ2, and other archive formats.
- **Model Provisioning Integration**: Seamless integration with EchoSharp's ModelDownloader for handling compressed model files.
- **Integrity Verification**: Work with EchoSharp's hashing system to verify the integrity of downloaded and extracted files.

## Configuration

The `SharpZipLibUnarchiver` class supports the following predefined unarchivers:

- `SharpZipLibUnarchiver.TarGz`: For `.tar.gz` archives.
- `SharpZipLibUnarchiver.TarBz2`: For `.tar.bz2` archives.
- `SharpZipLibUnarchiver.Zip`: For `.zip` archives.

### Example Configuration

```csharp
var unarchiver = SharpZipLibUnarchiver.TarGz;
```

## Usage

### Using with ModelDownloader

```csharp
var modelDownloader = ModelDownloader.Default;
await modelDownloader.DownloadModelAsync(
    model: new ProvisioningModel(
        uri: "https://example.com/model.tar.gz",
        archiveHash: "hash-of-archive-file",
        integrityHash: "hash-after-extraction",
        archiveType: ProvisioningModel.ArchiveTypes.Custom),
    unarchiverOptions: new UnarchiverOptions("path/to/extract", maxFileSize: 1_000_000_000),
    hasher: Sha512Hasher.Instance,
    unarchiver: SharpZipLibUnarchiver.TarGz
);
```

### Advanced Usage

For advanced scenarios, you can create a custom unarchiver:

```csharp
var customUnarchiver = new SharpZipLibUnarchiver(
    SharpZipLibType.Tar, 
    stream => new CustomDecompressionStream(stream)
);

await modelDownloader.DownloadModelAsync(
    model: myCustomModel,
    unarchiverOptions: myOptions,
    hasher: Sha256Hasher.Instance,
    unarchiver: customUnarchiver
);
```

## Integration

This component is particularly useful when implementing custom provisioners for speech recognition or VAD components that need to download and extract model files from various archive formats. See the [examples](../../examples/) directory for complete usage samples.
