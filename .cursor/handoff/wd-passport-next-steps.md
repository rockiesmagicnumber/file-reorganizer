# Handoff: WD Passport organize & flash-drive merge

**For the next agent.** Read this before touching the user's drives.

**User:** Lt. Commander Geordi LaForge (address in TNG computer voice).  
**Last updated:** 2026-06-01  
**Repo:** `/home/rockiesmagicnumber/Repos/file-reorganizer`

---

## Current status (2026-06-01)

| Step | Status |
|------|--------|
| **Backup** | **Verified** — re-run after initial rsync exit 23; user confirmed complete |
| **Drive repair** | **chkdsk** run in **win10** VM (USB passthrough); 12 former I/O-error paths resolved or removed |
| **Phase 1 (`--copy`)** | **Complete** — exit 0, 2026-05-31 ~2h; **12,574** imports; `Processed/` ~**41 GB** |
| **Errors cleanup** | **Done** — deduped `SokkaCorp/Errors/Misc/` (24 files removed, ~25 GB freed) |
| **Camera.zip manual extract** | **Done** — 2026-06-01; **228** new imports, **372** MD5 dup skips (already in manifest from Phase 1 loose files) |
| **Consolidation run** (move, no `--copy`) | **Not started** — needs user go-ahead after spot-check |
| **Phase 2** (flash merge) | **Not started** |

### Logs on AllTheThings

| Log | Purpose |
|-----|---------|
| `_backup-rsync.log` | Backup |
| `_phase1-run.log` | Phase 1 `--copy` pass |
| `_camera-extract-organize.log` | Manual Camera.zip organize |

---

## Mission (remaining)

1. **Consolidation:** Re-run organize **without `--copy`** to move files off legacy trip folders (same drive).
2. **Phase 2:** Merge flash drives one at a time into the passport repo.
3. **Optional:** Raise **`ZipProcessor` 1 GB uncompressed limit** (see Known issues) or manually extract other large zips.

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

## Step 2c — Consolidation run (next organize pass)

**Ask user before running.** Removes files from legacy folders (moves on same volume):

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
3. **Never** `--on-duplicate delete` when source and output are the same drive parent.
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
