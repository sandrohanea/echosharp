# EchoSharp Copilot instructions

EchoSharp is a near-real-time audio orchestration library. The core package in `src\EchoSharp` owns audio abstractions, speech transcription contracts, VAD contracts, realtime orchestration, and model provisioning. First-party integrations live under `components\` and must stay interchangeable through the core interfaces.

Focused repo skills live in `.github\skills`.

## Architecture rules

- Keep `src\EchoSharp` provider-neutral. It should not reference Whisper.net, ONNX Runtime, Azure, OpenAI, NAudio, WebRTC, Sherpa, or other component-specific packages.
- New speech-to-text components implement `ISpeechTranscriptorFactory` and `ISpeechTranscriptor`.
- New voice activity detection components implement `IVadDetectorFactory` and `IVadDetector`.
- Model-backed components should also provide a provisioner (`ISpeechTranscriptorProvisioner` or `IVadDetectorProvisioner`) that uses `ModelDownloader`, `ProvisioningModel`, `Sha512Hasher`, and the appropriate unarchiver.
- Match existing audio expectations unless the component explicitly documents otherwise: VAD and local Whisper components operate on mono 16 kHz audio sources.
- Keep package versions centralized in `Directory.Packages.props`; do not add inline `Version` attributes to `PackageReference` items.

## Component conventions

- Component projects live in `components\EchoSharp.<provider>.<technology>` and reference `src\EchoSharp\EchoSharp.csproj`.
- Public config classes hold user-facing settings and sensible defaults. Runtime-only classes can be internal.
- Factories own reusable model/runtime state. Detectors/transcriptors created from factories own per-run processors and dispose them.
- Provisioners should support both persisted model paths and in-memory models when the underlying engine supports both. If an engine only supports file paths, fail with a clear exception and document the limitation.
- Preserve the package packaging pattern: include root `LICENSE` and `readme.md` as packable `None` items.

## Validation workflow

- Run `dotnet restore .\EchoSharp.slnx /tl:off /nr:false`, `dotnet build .\EchoSharp.slnx --no-restore /tl:off /nr:false`, and `dotnet test .\EchoSharp.slnx --no-build /tl:off /nr:false` when changing code or packages.
- `EchoSharp.slnx` is the default development solution and intentionally excludes examples because the example executables copy large native runtime payloads. When changing examples, also run `dotnet restore .\EchoSharp.Examples.slnx /tl:off /nr:false` and `dotnet build .\EchoSharp.Examples.slnx --no-restore /tl:off /nr:false /p:CopyLocalLockFileAssemblies=false`.
- If model-backed tests are involved, run `.\downloadModels.ps1` on Windows or `./downloadModels.sh` on Unix first.
- Treat Whisper.net managed and runtime packages as an atomic version set. Mismatched `Whisper.net`, `Whisper.net.Runtime`, and Whisper.net runtime packages can fail at native load time.
