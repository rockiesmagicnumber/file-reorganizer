#!/usr/bin/env bash
# Register the win10 libvirt domain (existing qcow on network storage).
set -euo pipefail

REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
XML="${REPO}/scripts/win10.xml"
DISK="/mnt/network-storage/VMs/win10.qcow2"
NVRAM_TEMPLATE="/usr/share/OVMF/OVMF_VARS_4M.fd"
CONNECT="${LIBVIRT_DEFAULT_URI:-qemu:///system}"

if [[ "${CONNECT}" == qemu:///system ]]; then
  NVRAM="/var/lib/libvirt/qemu/nvram/win10_VARS.fd"
  VIRSH=(sudo virsh --connect "${CONNECT}")
else
  NVRAM="${HOME}/.local/share/libvirt/qemu/nvram/win10_VARS.fd"
  VIRSH=(virsh --connect "${CONNECT}")
fi

if [[ ! -f "${DISK}" ]]; then
  echo "Missing disk: ${DISK}" >&2
  echo "Mount network storage and retry." >&2
  exit 1
fi

# libvirt-qemu (uid on NFS) must read the qcow; owner can tighten after define if desired.
if [[ ! -r "${DISK}" ]]; then
  echo "Disk not readable: ${DISK}" >&2
  exit 1
fi

if [[ ! -f "${NVRAM}" ]]; then
  echo "Creating UEFI NVRAM: ${NVRAM}"
  if [[ "${CONNECT}" == qemu:///system ]]; then
    sudo install -d -m 755 /var/lib/libvirt/qemu/nvram
    sudo install -m 644 "${NVRAM_TEMPLATE}" "${NVRAM}"
  else
    install -d -m 755 "$(dirname "${NVRAM}")"
    install -m 644 "${NVRAM_TEMPLATE}" "${NVRAM}"
  fi
fi

# Patch NVRAM path in a temp copy so session vs system paths match.
TMPXML="$(mktemp)"
sed "s|/var/lib/libvirt/qemu/nvram/win10_VARS.fd|${NVRAM}|g" "${XML}" > "${TMPXML}"
trap 'rm -f "${TMPXML}"' EXIT

echo "Defining domain win10 (${CONNECT}) from ${XML}"
"${VIRSH[@]}" define "${TMPXML}"

echo
"${VIRSH[@]}" dominfo win10
echo
echo "Start with:  virt-manager  (or: sudo virsh start win10)"
echo "For chkdsk on My Passport:"
echo "  1. sudo umount \"/media/rockiesmagicnumber/My Passport\""
echo "  2. virt-manager -> win10 -> Add USB -> WD My Passport 0827 (1058:0827)"
echo "  3. chkdsk X: /f /r  (Admin CMD inside Windows)"
