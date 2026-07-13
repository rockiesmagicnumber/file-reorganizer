# Handoff: WD Passport organize & flash-drive merge

**For the next agent.** Read this before touching the user's drives.

**User:** Lt. Commander Geordi LaForge (address in TNG computer voice).  
**Last updated:** 2026-07-12  
**Repo:** `/home/rockiesmagicnumber/Repos/file-reorganizer`

---

## Current status (2026-07-12)

| Step | Status |
|------|--------|
| **Backup** | **Verified** — re-run after initial rsync exit 23; user confirmed complete |
| **Drive repair** | **chkdsk** run in **win10** VM (USB passthrough); 12 former I/O-error paths resolved or removed |
| **Phase 1 (`--copy`)** | **Complete** — exit 0, 2026-05-31 ~2h; **12,574** imports; `Processed/` ~**41 GB** |
| **Errors cleanup** | **Done** — deduped `SokkaCorp/Errors/Misc/` (24 files removed, ~25 GB freed) |
| **Camera.zip manual extract** | **Done** — 2026-06-01; **228** new imports, **372** MD5 dup skips (already in manifest from Phase 1 loose files) |
| **Consolidation run** (move, no `--copy`) | **Ran exit 0** (2026-06-02 ~1h06m) but that specific pass used `--on-duplicate skip`, so it only logged skips without clearing anything. |
| **Legacy dedupe cleanup** | **Done** — 2026-06-02, ~22:00–23:15, `--on-duplicate quarantine`. **67,225** duplicate sources moved into `SokkaCorp/Duplicates/` (**227 GB**). All top-level legacy trip folders are now empty. **Re-run 2026-07-10 confirmed as a no-op** (0 new files touched, exit 0) — nothing left to quarantine. **Note:** the previous version of this doc said this step was "not started" — that was stale; the work had already happened, just not logged here. |
| **Phase 2** (flash merge) | **Not started** |

### `$RECYCLE.BIN` audit (2026-07-11/12) — closed out

Ran a standalone read-only MD5 scan (not the app — it hard-skips `$RECYCLE.BIN` by name) of every file under `original-original/$RECYCLE.BIN` and its nested copies, against `jsonBackup.json`. Also independently verified the manifest itself: all 12,802 entries point into `Processed/`, none circular. Result: **29,161 files scanned, all accounted for** — 24,256 matched an existing manifest hash, 4,884 were pure OS metadata (`$I*` stubs, `._*` macOS sidecars), and exactly **3 unique files (1.8 MB) were genuinely new**: two Allstate insurance-claim PDFs (`ALFP126.pdf`, `JENNIFER_MARTIN3_...pdf`, both from 2015, deleted from an `F:` drive 2020-11-22) plus one empty 22-byte stub, each duplicated 7x across the nested `CHRISTMAS SMORES.doc/CHRISTMAS SMORES.doc/...` folder-loop bug. Also fully extracted and hash-verified the leftover `Wedding Photos 2-20200719T171437Z-001.zip` (36 real photos) — zero new content.

**User decision:** discard the Allstate PDFs rather than recover them (not photos/media, just old insurance paperwork). Deleted 2026-07-11/12:
- All 42 files (3 unique × 7 nested locations × `$R` content + `$I` metadata pairs) under the `CHRISTMAS SMORES.doc` recursion.
- 12 orphaned `$I*.zip` metadata-stub copies sitting in `SokkaCorp/Errors/Misc/` (leftover from an April 2026 import attempt that failed because `$I` files aren't valid zips despite the extension).

**`SokkaCorp/Duplicates/`** (67,225 files, 227 GB — every one with a verified twin in `Processed/`) was also deleted 2026-07-11/12 to reclaim space. Not required for the tool to function; `DirectoryManager` recreates it on demand next time something needs quarantining.

**Net result:** drive usage dropped from **390 GB → 164 GB used** (768 GB free). `Processed/` (41 GB) untouched throughout.

### What's actually left on the WD drive outside `MediaLibrary/`/`Original/`

- `original-original/` is now just empty legacy folder shells + `$RECYCLE.BIN`/System Volume Information OS internals (System Volume Information itself is ~38 KB, negligible — mostly index GUIDs, tracking logs, VSS restore-point data, and a chkdsk log from the 2026-05-31 drive repair). None of this is user media; the tool is designed to never import it.
- 12,608 legacy directories remain, mostly now-empty shells from the old trip-folder layout — cosmetic only, no space to reclaim by removing them.

### Logs on AllTheThings

| Log | Purpose |
|-----|---------|
| `_backup-rsync.log` | Backup |
| `_phase1-run.log` | Phase 1 `--copy` pass |
| `_camera-extract-organize.log` | Manual Camera.zip organize |
| `_phase1-consolidation-run.log` | Consolidation (move) pass — includes interrupted 2026-06-01 run + successful RE-RUN |

---

## Mission (remaining)

1. **Phase 2:** Merge flash drives one at a time into the passport repo.
2. **Optional:** Delete the now-empty legacy trip-folder shells (~12.6k dirs) — cosmetic only, no space to reclaim. **Ask user first.**
3. **Optional:** Raise **`ZipProcessor` 1 GB uncompressed limit** (see Known issues) or manually extract other large zips.

~~Decide fate of `SokkaCorp/Duplicates/`~~ — **done 2026-07-11/12**, deleted, 226 GB reclaimed. See audit section above.

---

## Drives & paths

| Role | Path | Notes |
|------|------|--------|
| **Source / repo (WD)** | `/media/rockiesmagicnumber/My Passport` | **~414+ GB** used; **`MediaLibrary/` exists** |
| **Backup mirror** | `/media/all-the-things/Backups/My-Passport-2026-05-30/` | rsync from [`scripts/backup-my-passport.sh`](../scripts/backup-my-passport.sh) |
| **Release binary** | `MediaLibraryReorganizer/bin/Release/net8.0/MediaLibraryOrganizer` | Build from repo root |
| **win10 VM (chkdsk)** | `qemu:///session`, disk `/mnt/network-storage/VMs/win10.qcow2` | See [`scripts/win10.xml`](../scripts/win10.xml), [`scripts/define-win10-vm.sh`](../scripts/define-win10-vm.sh) — **BIOS boot**, user session in virt-manager |

```bash
test -d "/media/rockiesmagicnumber/My Passport" && test -d "/media/all-the-things/Backups/My-Passport-2026-05-30"
```

---

## Layout on passport (after Phase 1)

```text
/media/rockiesmagicnumber/My Passport/
  MediaLibrary/
    Processed/              ← ~41 GB organized media
    SokkaCorp/
      Logs/
      jsonBackup.json       ← --track manifest
      Errors/Misc/
        Camera_20260418131012.zip   ← sole kept copy (~2.1 GB); contents mostly already in Processed
  Original/                 ← empty
  (legacy trip/event folders) ← still populated (--copy pass)
```

---

## Step 2b — Manual large zip extract (pattern used for Camera.zip)

**Do not extract under `MediaLibrary/`** — same-drive scan skips the output tree (`IsUnderOutputTree`).

```bash
ZIP="/media/rockiesmagicnumber/My Passport/MediaLibrary/SokkaCorp/Errors/Misc/Camera_20260418131012.zip"
EXTRACT="/media/rockiesmagicnumber/My Passport/_camera-extract"   # outside MediaLibrary/
mkdir -p "$EXTRACT"
unzip -o "$ZIP" -d "$EXTRACT"

./MediaLibraryReorganizer/bin/Release/net8.0/MediaLibraryOrganizer \
  --source "$EXTRACT" \
  --output "/media/rockiesmagicnumber/My Passport" \
  --track --on-duplicate skip

rm -rf "$EXTRACT"   # only after verifying imports / dup skips in log
```

---

## Step 2c — Consolidation run (2026-06-02)

**Scanner pass complete (exit 0)** but **`--on-duplicate skip` does not move or delete** sources already in `jsonBackup.json`. Phase 1 `--copy` already placed media under `MediaLibrary/Processed/{Photos,Videos,Music,Misc}/…`. This pass only logged skips; ~96k files still under `original-original/`.

**Next — legacy dedupe cleanup** (ask user):

```bash
./MediaLibraryReorganizer/bin/Release/net8.0/MediaLibraryOrganizer \
  --source "/media/rockiesmagicnumber/My Passport" \
  --output "/media/rockiesmagicnumber/My Passport" \
  --track \
  --on-duplicate quarantine \
  --interactive
```

Use `delete` instead of `quarantine` only with explicit approval (faster, no `Duplicates/` copies).

Reference — original consolidation command (skip-only; does not clear legacy):

```bash
./MediaLibraryReorganizer/bin/Release/net8.0/MediaLibraryOrganizer \
  --source "/media/rockiesmagicnumber/My Passport" \
  --output "/media/rockiesmagicnumber/My Passport" \
  --track \
  --on-duplicate skip \
  --interactive
```

Omit `--copy`. Omit `--interactive` only if unattended and errors → `SokkaCorp/Errors/` is acceptable.

---

## Step 3 — Phase 2: Merge flash drives

For each flash mount under `/media/rockiesmagicnumber/`:

```bash
./MediaLibraryReorganizer/bin/Release/net8.0/MediaLibraryOrganizer \
  --source "/media/rockiesmagicnumber/FLASH_MOUNT_NAME" \
  --output "/media/rockiesmagicnumber/My Passport" \
  --track \
  --on-duplicate skip \
  --interactive
```

On success, `--interactive` offers flash wipe (default **No**).

---

## Known issues / lessons

| Issue | Detail |
|-------|--------|
| **Zip 1 GB limit** | `ZipProcessor.MaxUncompressedSize` = 1 GB — rejects legit **`Camera.zip`** (~2 GB uncompressed). No CLI override yet. Validation failure → copy to `SokkaCorp/Errors/`. |
| **Duplicate Camera copies in Errors** | Phase 1 `--copy` + repeated nested paths copied same 2.1 GB zip many times; **cleaned 2026-06-01** (keep one). |
| **Corrupt `$I*.zip` stubs** | Windows temp fragments (~98 B) in legacy `Errors/Misc` — **removed**; safe to ignore if seen elsewhere. |
| **pgrep monitor false positive** | Background watch scripts matching `pgrep -af 'rsync.*My Passport'` or `MediaLibraryOrganizer` can match their own shell — use `pgrep -x rsync` or check finish markers in logs. |
| **USB / metadata stalls** | Occasional ~108 s `ProcessPhoto` waits = USB I/O retry, not CPU. |
| **win10 VM** | Legacy **MBR/BIOS** disk — use **SeaBIOS**, not OVMF. Session libvirt for USB; Spice USB redirect if host passthrough permission denied. |

---

## Safety rules (non-negotiable)

1. **Rollback:** `/media/all-the-things/Backups/My-Passport-2026-05-30/` — ask before overwriting WD.
2. **No destructive ops** on WD (delete legacy folders, wipe, bulk rm) without **explicit user approval**.
3. **`--on-duplicate delete`** on same drive is OK **only** when `Processed/` already holds the manifest copy (post `--copy` dedupe). Do not use `delete` on first import of unknown files.
4. **Manual extract scratch** must live **outside** `MediaLibrary/` and `Original/`.

---

## Ask the user before

- Consolidation run (drop `--copy`)
- Deleting empty legacy folders on WD
- Deleting the kept `Camera_*.zip` in Errors (photos largely already in `Processed`)
- Wiping a flash drive
- Any git commit or push

---

## Related docs

- [`AGENTS.md`](../AGENTS.md) — repo map, build, behavior
- [`README.md`](../README.md) — CLI reference
- [`scripts/backup-my-passport.sh`](../scripts/backup-my-passport.sh)
- [`scripts/phase1-after-backup.sh`](../scripts/phase1-after-backup.sh) — automation (backup gate + Phase 1)
