# file-reorganizer

Console tool for rearranging loose media into predictable folder layouts.

## Project status

- **Media library reorganizer:** Actively maintained path — Core (`netstandard2.1`) + CLI (`net8.0`). A full **`dotnet build FileReorganizer.sln -c Release`** completes cleanly.
- **Tests & CI:** There is **no test project** and **no in-repo GitHub Actions** workflow at present — regression coverage is manual.

---

## Media Library Reorganizer (primary)

Cross-platform CLI that scans a source tree (for example a mounted backup drive). The **`--output`** path is a **parent folder**: the tool creates **`MediaLibrary/`** (user media under `Processed/`; tool artifacts under `SokkaCorp/`) and **`Original/`** (created empty—you can use it as a dedicated unorganized archive so only those two names sit at the output parent). ZIPs expand under **`MediaLibrary/SokkaCorp/Unzipped`** (short-lived scratch per archive). Files **move** into `MediaLibrary/Processed/...` when the source is writable. Use **`--copy`** to always **copy** instead of move (sources stay in place). Read-only files are copied even without `--copy`. Legacy libraries with `Errors/`, `Duplicates/`, or `Unzipped/` at the `MediaLibrary/` root are auto-migrated into `SokkaCorp/` on startup.

- **Photos / videos:** `Processed/Photos|Videos/YYYY/MM/DD/filename`. Photos use TagLib dates, then MetadataExtractor EXIF-style tags, then **last write time** when metadata is missing. If the destination name already exists, `_1`, `_2`, … is appended before the extension.
- **Music:** `Processed/Music/Artist/Album/track.ext` from TagLib — album artist, performer, joined strings, then legacy `Artists` (YouTube Music / Takeout–friendly); falls back to “Unknown Artist/Album” with an informational log when tags are missing.
- **Other:** `Processed/Misc/YYYY/MM/DD/filename`.
- **ZIPs:** Left on the source when possible; each archive is extracted, its tree is walked (nested ZIPs use the same pattern), then that scratch folder is deleted so peak disk use stays near **one expansion** (plus nested archives while they are open). The same archive identity is not expanded twice in a single run. Loose `.zip` files are not imported into `Processed`.

**Checksums (optional):** With `--track`, content keys use **MD5** (hex, no separators) for the manifest and duplicate detection. Without `--track`, no manifest I/O and ZIP de-dupe uses path/size/time instead of hashing.

**Solution / layout**

- Single solution at repo root: [`FileReorganizer.sln`](FileReorganizer.sln) (media library Core + host).
- **`MediaLibraryReorganizer/MediaLibraryOrganizer.Core`** — `netstandard2.1` class library.
- **`MediaLibraryReorganizer/MediaLibraryOrganizer`** — `net8.0` media library CLI.

Assistants and contributors: see **`AGENTS.md`** for behaviour detail, pitfalls, and the host/Core compile-glob rule.

**CLI**

| Option | Description |
|--------|-------------|
| `-s`, `--source` | Required. Root directory to scan (must exist). |
| `-o`, `--output` | Optional. **Parent** folder; default **Documents**. Creates **`MediaLibrary/`** and **`Original/`** under it. |
| `--copy` | Copy into the library instead of moving (non-destructive on source). |
| `--track` | Enable `{output}/MediaLibrary/SokkaCorp/jsonBackup.json`, MD5 duplicate detection, and MD5-based ZIP identity in a run. |
| `--on-duplicate` | With `--track`: `skip` (default), `delete`, or `quarantine` (move/copy duplicate **sources** to `{output}/MediaLibrary/SokkaCorp/Duplicates/`). |
| `--refresh-json` | With `--track`: prune manifest entries whose files no longer exist. |
| `--repopulate-json` | With `--track`: rebuild manifest from `MediaLibrary/Processed/`. |
| `--max-zip-depth` | Stop expanding nested `.zip` at this nesting index (default `5`; outermost archives use `0`). Use `0` for no limit. |

Example:

```bash
dotnet run --project MediaLibraryReorganizer/MediaLibraryOrganizer.csproj -- \
  --source /Volumes/Backup/DCIM \
  --output /Volumes/Media/MyLibrary \
  --copy \
  --track \
  --on-duplicate quarantine
```

Release binary:

```bash
dotnet build FileReorganizer.sln -c Release
./MediaLibraryReorganizer/bin/Release/net8.0/MediaLibraryOrganizer -- --source … --output …
```

Or build only the app: **`dotnet build MediaLibraryReorganizer/MediaLibraryOrganizer.csproj -c Release`**.

**Manifest (only with `--track`):** `{output}/MediaLibrary/SokkaCorp/jsonBackup.json` maps MD5 → destination paths. First instance of a hash wins; later files follow `--on-duplicate`.

**Logs:** `{output}/MediaLibrary/SokkaCorp/Logs/` (Serilog). The host resolves `-o` to `{output}/MediaLibrary` for the file log path.

**Migrating:** If you previously set `-o` to a folder that already **was** the library root (containing `Processed/` at the top), point `-o` at its **parent** so `Processed/` lives under `MediaLibrary/` (or move your tree into `MediaLibrary/` once and use the parent as `-o`).

**Google Takeout / YouTube Music:** use the CLI above (`-s` / `-o`); ZIPs expand under **`MediaLibrary/SokkaCorp/Unzipped`** and tracks are picked up recursively (for example under `…/Takeout/YouTube and YouTube Music/music-uploads/`).

---

## Roadmap (ideas)

- Automated tests + optional CI (`dotnet build` / targeted integration runs).
- SQLite or other manifest store.
