#!/usr/bin/env bash
# Full mirror backup of the WD "My Passport" drive before running MediaLibraryOrganizer.
# Non-destructive: read-only on source; writes only to BACKUP_ROOT.

set -euo pipefail

SOURCE="${SOURCE:-/media/rockiesmagicnumber/My Passport}"
BACKUP_ROOT="${BACKUP_ROOT:-/media/all-the-things/Backups}"
STAMP="${STAMP:-$(date +%Y-%m-%d)}"
DEST="${DEST:-${BACKUP_ROOT}/My-Passport-${STAMP}}"
LOG="${LOG:-${DEST}/_backup-rsync.log}"
DRY_RUN="${DRY_RUN:-0}"

RSYNC_FLAGS=(
  -aH
  --human-readable
  --info=progress2
  --exclude='$RECYCLE.BIN/'
  --exclude='System Volume Information/'
  --exclude='.Trash*/'
  --exclude='__MACOSX/'
)

if [[ "${DRY_RUN}" == "1" ]]; then
  RSYNC_FLAGS+=(--dry-run)
fi

if [[ ! -d "${SOURCE}" ]]; then
  echo "Source not mounted: ${SOURCE}" >&2
  exit 1
fi

mkdir -p "${BACKUP_ROOT}" "${DEST}"

echo "Source:  ${SOURCE}"
echo "Dest:    ${DEST}"
echo "Log:     ${LOG}"
echo "Dry run: ${DRY_RUN}"
echo

if [[ "${DRY_RUN}" == "1" ]]; then
  rsync "${RSYNC_FLAGS[@]}" "${SOURCE}/" "${DEST}/" | tee -a "${LOG}"
  echo "Dry run complete. Re-run with DRY_RUN=0 to copy."
  exit 0
fi

{
  echo "=== backup started $(date -Iseconds) ==="
  rsync "${RSYNC_FLAGS[@]}" "${SOURCE}/" "${DEST}/"
  echo "=== backup finished $(date -Iseconds) ==="
} 2>&1 | tee -a "${LOG}"

echo
echo "Backup complete."
echo "Verify with: du -sh \"${SOURCE}\" \"${DEST}\""
