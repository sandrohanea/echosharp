---
applyTo: "**/*.{csproj,props,targets,sln,yml,yaml,ps1,sh}"
---

# Dependency maintenance skill

Use central package management only. Versions belong in `Directory.Packages.props`; project files should contain versionless `PackageReference` entries.

When updating packages:

- Use the NuGet MCP planning tools for production projects before editing package versions.
- Exclude test projects, examples, benchmarks, and samples from NuGet MCP production project inputs.
- Keep native/runtime package families version-aligned, especially Whisper.net managed/runtime packages.
- Verify target framework compatibility for `net8.0` and `net10.0` before accepting a latest version.
- Prefer removing obsolete transitive pins when the plan recommends removal rather than blindly bumping them.
- Fix malformed package ids while touching central versions, for example package names with accidental trailing spaces.

After dependency edits, restore before building so generated assets reflect the new graph.
