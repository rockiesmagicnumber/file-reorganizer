# Agent notes — file-reorganizer

Context for automated assistants and contributors working in this repository.

## Active handoff (2026-06-01)

**WD Passport organize + flash merge** — Phase 1 `--copy` **complete**; Camera.zip manual extract **done**; **consolidation run** (move) and **Phase 2** flash merge **pending** user go-ahead.  
→ **[`.cursor/handoff/wd-passport-next-steps.md`](.cursor/handoff/wd-passport-next-steps.md)** (full runbook for the next agent).

## Current status

| Area | State |
|------|--------|
| **Media library CLI + Core** | **Feature-complete** for the documented workflow: directory walk, ZIP expansion with nesting limits & run-scope dedupe, photo/video/music/misc routing, optional MD5 manifest + duplicate disposition, Serilog hosting. **`dotnet build FileReorganizer.sln -c Release`** succeeds with **zero warnings**. |
| **Automated tests** | **None** — solution has two projects (Core + media host); no test project or CI workflows in-repo. |
| **CI / GitHub Actions** | **Absent** — no `.github/workflows` in the tree checked at last audit. |

### Mature / stable

- Core orchestration: [`MediaLibraryOrganizer`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/MediaLibraryOrganizer.Lib.cs), [`FileProcessor`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/FileProcessor.cs), [`ZipProcessor`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/ZipProcessor.cs), [`BackupManager`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/BackupManager.cs), [`DirectoryManager`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/DirectoryManager.cs).
- Host wiring: [`Program.cs`](MediaLibraryReorganizer/Program.cs), [`CommandLineOptions.cs`](MediaLibraryReorganizer/CommandLineOptions.cs) (early log path prime, `--track` gated JSON maintenance modes).

### Worth attention / known rough edges

- **[`FileDictionary.CopyTo`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/FileDictionary.cs)** throws **`NotImplementedException`** — unused today; harmless unless something treats the collection as **`IDictionary<,>`** and calls **`CopyTo`**.
- **`FileProcessor.ProcessErrorFile`**: collision-avoidance path layout is unconventional — review if error-folder collisions matter in practice.
- **Video extensions**: **`.ogg`** is commented out in [`Constants`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/Constants.cs); intentional omission vs bug is unclear without product context.

### Historical note

Legacy **`MusicReorganizer`** (separate exe) and **`WallpaperReorganizer`** were removed from the tree — music and images are handled by the **media library** Core paths. Do not resurrect unless there is an explicit requirement.

---

## Repository map

| Path | Notes |
|------|--------|
| [`MediaLibraryReorganizer/`](MediaLibraryReorganizer/) | **Primary** tool: flash-drive / backup → organized library tree. |
| [`MediaLibraryReorganizer/MediaLibraryOrganizer.Core/`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/) | `netstandard2.1` DLL (`SokkaCorp.MediaLibraryOrganizer.Core`). Organizer logic, metadata, ZIP expansion, optional JSON manifest. |
| [`MediaLibraryReorganizer/MediaLibraryOrganizer.csproj`](MediaLibraryReorganizer/MediaLibraryOrganizer.csproj) | `net8.0` executable; Serilog sinks; CommandLineParser; **project-references** Core. |
| [`FileReorganizer.sln`](FileReorganizer.sln) | **Only solution** — Core + media library host. |

Namespace for core logic: `SokkaCorp.MediaLibraryOrganizer.Lib`. Host CLI types: `SokkaCorp.MediaLibraryOrganizer`.

## Build

```bash
dotnet build FileReorganizer.sln -c Release
```

From the repo root. The media library host (`MediaLibraryReorganizer/MediaLibraryOrganizer.csproj`) **must** exclude Core sources from its compile glob (`<Compile Remove="MediaLibraryOrganizer.Core\**\*.cs" />`); otherwise types compile twice and produce CS0436 / CS0121 errors.

If you open **`MediaLibraryReorganizer/`** as the VS Code workspace folder, build tasks use `../FileReorganizer.sln`.

## Layout and behavior

- **Destination paths:** CLI `-o` is the **output parent**; [`CommandLineOptions.Execute`](MediaLibraryReorganizer/CommandLineOptions.cs) creates `{parent}/MediaLibrary` and `{parent}/Original`, then passes **`MediaLibrary`** into [`DirectoryManager`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/DirectoryManager.cs). User media lives under `Processed/`; tool artifacts (logs, manifest, `Errors/`, `Duplicates/`, `Unzipped/` scratch) live under `SokkaCorp/`. [`ToolLayoutMigrator`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/ToolLayoutMigrator.cs) auto-moves legacy sibling tool folders into `SokkaCorp/` on startup. `Original/` is not populated by the tool.
- **[`PhotoTakenDate`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/PhotoTakenDate.cs)** resolves photo folder dates (TagLib, then MetadataExtractor EXIF-style tags, then last write time). **[`OrganizerOptions.CopyOnly`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/OrganizerOptions.cs)** (`--copy`) forces copy instead of move for imports, duplicate quarantine, and bad ZIPs to the error folder. [`FileProcessor`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/FileProcessor.cs) uniquifies destination filenames when the target path already exists.
- **[`Statics`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/Statics.cs)** supplies `GetLogFilePath`, `GetChecksum`, and file-type extension helpers.
- **ZIPs:** [`ZipProcessor.ExtractProcessAndCleanup`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/ZipProcessor.cs) expands one archive at a time; [`MediaLibraryOrganizer.ProcessDirectoryNode`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/MediaLibraryOrganizer.Lib.cs) walks the source tree directory-by-directory so expansion is not driven by a flat “all `.zip` under tree” scan (avoids re-processing the same path). Nesting cutoff: CLI `--max-zip-depth` → [`OrganizerOptions.ZipNestingDepthCutoff`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/OrganizerOptions.cs) (`0` = unlimited). Identity dedupe unchanged.
- **No full-tree staging:** read-only sources use **per-file copy** into `Processed/` when the file has the read-only attribute; writable sources use **move**.
- **`--track`:** enables [`BackupManager`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/BackupManager.cs) manifest + [`FileProcessor`](MediaLibraryReorganizer/MediaLibraryOrganizer.Core/FileProcessor.cs) MD5 duplicate handling (`skip` / `delete` / `quarantine`).

## Publishing without a shared .NET runtime (e.g. macOS)

`netstandard` applies only to the **library**. The runnable app is `net8.0`. End users who do not want to install the .NET runtime should use **self-contained** publish (runtime bundled):

Apple Silicon:

```bash
dotnet publish MediaLibraryReorganizer/MediaLibraryOrganizer.csproj -c Release -r osx-arm64 --self-contained true
```

Intel Mac:

```bash
dotnet publish MediaLibraryReorganizer/MediaLibraryOrganizer.csproj -c Release -r osx-x64 --self-contained true
```

Linux x64:

```bash
dotnet publish MediaLibraryReorganizer/MediaLibraryOrganizer.csproj -c Release -r linux-x64 --self-contained true
```

Optional: add `/p:PublishSingleFile=true` for a single-file binary (startup cost tradeoff).

## Conventions

- **Checksums:** MD5 hex for manifest keys when `--track` is used.
- **No StyleCop** in this solution (removed from Core and host csproj).

## Dependencies (Core)

- `TagLibSharp`, `metadataextractor`, `Serilog`, `System.Text.Json`

## Dependencies (host only)

- `CommandLineParser`, `Serilog.Sinks.Console`, `Serilog.Sinks.File`
