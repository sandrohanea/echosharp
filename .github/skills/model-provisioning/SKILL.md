---
applyTo: "components/**,src/EchoSharp/Provisioning/**,tests/**,downloadModels.*"
---

# Model provisioning skill

Use EchoSharp's provisioning pipeline for downloadable models.

- Represent downloadable models with `ProvisioningModel` subclasses and static catalogs.
- Pin model URLs to immutable versions or tags where possible.
- Store SHA-512 archive hashes and integrity hashes in base64, along with archive and max-file sizes.
- Use `ModelDownloader.DownloadModelAsync` with `Sha512Hasher.Instance`.
- Use `UnarchiverCopy.Instance` for single-file models, `UnarchiverZip.Instance` for zip archives, and component-specific unarchivers for other formats.
- Persisted model downloads write into a directory and the copied model file is named with `UnarchiverCopy.ModelName`; pass `Path.Combine(config.ModelPath, UnarchiverCopy.ModelName)` to APIs that need a file path.
- Add test/download script and `CopyModels.targets` entries for any model required by integration tests.
