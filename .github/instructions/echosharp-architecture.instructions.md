---
applyTo: "**/*.{cs,csproj,props,targets,sln,md,ps1,sh,yml,yaml}"
---

# EchoSharp architecture skill

Use this when changing architecture, adding components, or reviewing code in this repository.

EchoSharp separates provider-neutral orchestration from provider-specific integrations:

- `src\EchoSharp\Audio`: reusable audio sources and sample conversion utilities.
- `src\EchoSharp\SpeechTranscription`: STT contracts, realtime orchestration, and transcript models.
- `src\EchoSharp\VoiceActivityDetection`: VAD contracts and segment models.
- `src\EchoSharp\Provisioning`: model download, integrity, hashing, memory/file model storage, and unarchive helpers.
- `components\*`: provider-specific adapters that plug into core contracts.
- `tests\EchoSharp.Tests`: integration and unit coverage for core and component behavior.

When adding a component, mirror the closest existing component before inventing a new pattern. The usual shape is:

1. Public config class for provisioner settings.
2. Public options class for factory/runtime settings.
3. Public factory implementing the core factory interface.
4. Internal detector/transcriptor implementing the core runtime interface.
5. Public provisioner implementing the matching provisioning interface.
6. Static model catalog when models can be downloaded with integrity checks.

Keep dependencies flowing inward: components depend on core; core never depends on components.
