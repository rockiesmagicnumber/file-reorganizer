# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A cross-platform .NET console tool (`MediaLibraryReorganizer`) that scans a source tree (e.g. a mounted backup drive) and reorganizes loose media into a predictable folder layout, sorting photos/videos by date, music by artist/album, and everything else into misc. It handles ZIP expansion (including nested archives), optional MD5-based duplicate tracking, and structured logging via Serilog.

There is **no test project and no CI** in this repo — regression coverage is manual. See `AGENTS.md` for the fuller behavior/pitfalls writeup this file summarizes.

## Build

```bash
dotnet build FileReorganizer.sln -c Release
```

Run from the repo root — `FileReorganizer.sln` is the only solution (Core library + CLI host).

**Critical build constraint:** `MediaLibraryReorganizer/MediaLibraryOrganizer.csproj` must exclude Core sources from its compile glob (`<Compile Remove="MediaLibraryOrganizer.Core\**\*.cs" />`, already present) — otherwise types compile twice, producing CS0436/CS0121 errors. If you ever restructure these two projects, preserve this exclusion.

Build only the app: `dotnet build MediaLibraryReorganizer/MediaLibraryOrganizer.csproj -c Release`

## Run

```bash
dotnet run --project MediaLibraryReorganizer/MediaLibraryOrganizer.csproj -- \
  --source /path/to/source \
  --output /path/to/output-parent \
  --copy \
  --track \
  --on-duplicate quarantine
```

Key CLI options (`MediaLibraryReorganizer/CommandLineOptions.cs`):
- `-s`/`--source` (required), `-o`/`--output` (parent folder; default Documents)
- `--copy` — copy instead of move (source untouched)
- `--track` — enables the JSON manifest + MD5 duplicate detection
- `--on-duplicate` — `skip` (default) / `delete` / `quarantine`, only meaningful with `--track`
- `--refresh-json` / `--repopulate-json` — manifest maintenance modes, gated on `--track`
- `--max-zip-depth` — nested-zip expansion cutoff (default 5, `0` = unlimited)

There's sample input/output under `test-data/from-downloads` and `test-data/from-downloads-out` useful for manual smoke tests of a run.

## Publishing self-contained binaries

The CLI targets `net8.0`; the Core library targets `netstandard2.1`. For end users without a shared .NET runtime, publish self-contained:

```bash
dotnet publish MediaLibraryReorganizer/MediaLibraryOrganizer.csproj -c Release -r <rid> --self-contained true
```

RIDs: `osx-arm64`, `osx-x64`, `linux-x64`. Add `/p:PublishSingleFile=true` for a single binary.

## Architecture

Two projects under `MediaLibraryReorganizer/`:
- **`MediaLibraryOrganizer.Core`** (`netstandard2.1`, assembly `SokkaCorp.MediaLibraryOrganizer.Core`, namespace `SokkaCorp.MediaLibraryOrganizer.Lib`) — all organizer logic.
- **`MediaLibraryOrganizer`** (`net8.0` exe, namespace `SokkaCorp.MediaLibraryOrganizer`) — CLI host: arg parsing (`CommandLineOptions.cs`), logging setup and entry point (`Program.cs`).

Orchestration flow: `MediaLibraryOrganizer.Lib.cs`'s `MediaLibraryOrganizer.OrganizeFiles()` calls `ProcessDirectoryNode`, which walks the source tree **one directory level at a time** (not a flat recursive `.zip` scan) — non-ZIP files go to `FileProcessor.ProcessFile`, ZIPs go to `ZipProcessor.ExtractProcessAndCleanup` which expands to scratch, recurses into the extracted tree at `zipNestingDepth + 1`, then deletes the scratch folder. This keeps peak disk usage near one archive expansion at a time and avoids reprocessing the same path. Directories under the output tree itself (and recycle-bin/trash/system dirs) are skipped via `ShouldSkipDirectory`.

Core collaborators, all constructor-injected into `MediaLibraryOrganizer`:
- **`DirectoryManager`** — resolves source/output directories, manages the `Unzipped` scratch area.
- **`FileProcessor`** — routes a single file to its destination path (photo/video/music/misc), performs the move-or-copy, and uniquifies filenames on collision (`_1`, `_2`, …).
- **`ZipProcessor`** — extracts one archive at a time, tracks nesting depth against `OrganizerOptions.ZipNestingDepthCutoff`, cleans up scratch after processing.
- **`BackupManager`** — owns the optional JSON manifest (`jsonBackup.json`, MD5 hash → destination path) used for duplicate detection when `--track` is set.

Output layout (`-o` is the **parent**, not the library root itself):
- `{output}/MediaLibrary/Processed/{Photos,Videos}/YYYY/MM/DD/filename`, `Processed/Music/Artist/Album/track.ext`, `Processed/Misc/YYYY/MM/DD/filename`
- `{output}/MediaLibrary/SokkaCorp/` — tool artifacts: `Logs/`, `jsonBackup.json`, `Errors/`, `Duplicates/`, `Unzipped/` (scratch)
- `{output}/Original/` — created empty; not populated by the tool, available for the user's own unorganized archive
- `ToolLayoutMigrator` auto-migrates legacy libraries that have `Errors/`/`Duplicates/`/`Unzipped/` sitting directly at the `MediaLibrary/` root into `SokkaCorp/` on startup.

Move vs. copy: writable sources are **moved**; read-only sources are **copied** even without `--copy`; `--copy` forces copy everywhere (imports, duplicate quarantine, bad ZIPs routed to the error folder).

Date resolution for photos (`PhotoTakenDate.cs`): TagLib metadata → MetadataExtractor EXIF-style tags → file's last-write time as final fallback.

Music tagging (`FileProcessor`): TagLib album artist → performer → joined artist strings → legacy `Artists` field (for YouTube Music/Takeout exports) → falls back to "Unknown Artist/Album" with an info log.

Checksums: MD5 hex, no separators, used only when `--track` is set (manifest keys, duplicate detection, ZIP-archive identity within a run). Without `--track`, ZIP dedupe falls back to path/size/time and no manifest I/O happens.

## Known rough edges (see AGENTS.md for detail)

- `FileDictionary.CopyTo` throws `NotImplementedException` — unused currently but a landmine if something starts treating the collection as `IDictionary<,>`.
- `.ogg` is commented out of the video extensions in `Constants.cs` — unclear if intentional.
- Legacy `MusicReorganizer` and `WallpaperReorganizer` tools were removed from the tree; don't resurrect them without an explicit requirement — music/images are handled by the Core paths now.

## Conventions

- No StyleCop in this solution (removed from both csproj files).
- `Nullable` is enabled and `ImplicitUsings` is disabled in both projects — use explicit `using` directives.
