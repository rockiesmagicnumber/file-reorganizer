#!/usr/bin/env bash
# Wait for WD Passport backup to finish, verify, then run Phase 1 organize (--copy).
set -euo pipefail

REPO="/home/rockiesmagicnumber/Repos/file-reorganizer"
SOURCE="/media/rockiesmagicnumber/My Passport"
BACKUP="/media/all-the-things/Backups/My-Passport-2026-05-30"
BACKUP_LOG="${BACKUP}/_backup-rsync.log"
RUN_LOG="${BACKUP}/_phase1-after-backup.log"
ORGANIZER="${REPO}/MediaLibraryReorganizer/bin/Release/net8.0/MediaLibraryOrganizer"

exec > >(tee -a "${RUN_LOG}") 2>&1

log() { echo "[$(date -Iseconds)] $*"; }

log "=== phase1-after-backup: waiting for backup ==="

while true; do
  if grep -q '=== backup finished ' "${BACKUP_LOG}" 2>/dev/null; then
    break
  fi
  if ! pgrep -af 'rsync.*My Passport' 2>/dev/null | grep -q 'rsync -aH'; then
    log "=== PHASE1_FAILED backup stopped without finish marker ==="
    exit 1
  fi
  SZ=$(du -sh "${BACKUP}" 2>/dev/null | cut -f1)
  log "backup in progress, mirror size=${SZ}"
  sleep 120
done

log "=== backup finished marker found ==="
grep '=== backup finished ' "${BACKUP_LOG}" | tail -1

SRC_DF=$(df -h "${SOURCE}" | tail -1)
BKUP_DU=$(du -sh "${BACKUP}" | cut -f1)
log "source: ${SRC_DF}"
log "backup: ${BKUP_DU}"

log "=== building Release ==="
dotnet build "${REPO}/FileReorganizer.sln" -c Release --nologo

if [[ ! -x "${ORGANIZER}" ]]; then
  log "=== PHASE1_FAILED organizer binary missing: ${ORGANIZER} ==="
  exit 1
fi

log "=== Phase 1 organize (--copy --track --on-duplicate skip; no --interactive while unattended) ==="
"${ORGANIZER}" \
  --source "${SOURCE}" \
  --output "${SOURCE}" \
  --track \
  --on-duplicate skip \
  --copy

log "=== PHASE1_COMPLETE ==="
log "Check Processed/: ${SOURCE}/MediaLibrary/Processed/"
log "Check logs: ${SOURCE}/MediaLibrary/SokkaCorp/Logs/"
