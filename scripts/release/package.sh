#!/usr/bin/env bash
set -euo pipefail
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
VERSION="${1:?Usage: package.sh <version> [linux|windows|macos|finalize]}"
TARGET="${2:-}"
CHANNEL="${WIISTUDIO_CHANNEL:-stable}"
REPOSITORY="${WIISTUDIO_REPOSITORY:-renatoaleixo/WiiStudio}"
if [[ -z "$TARGET" ]]; then
  case "$(uname -s)" in Linux) TARGET=linux;; Darwin) TARGET=macos;; MINGW*|MSYS*|CYGWIN*) TARGET=windows;; *) echo "Unsupported host." >&2; exit 2;; esac
fi
case "$TARGET" in
  linux) "$ROOT_DIR/scripts/release/publish.sh" "$VERSION" linux-x64; "$ROOT_DIR/scripts/release/package-linux.sh" "$VERSION" ;;
  macos) for rid in osx-x64 osx-arm64; do "$ROOT_DIR/scripts/release/publish.sh" "$VERSION" "$rid"; "$ROOT_DIR/scripts/release/package-macos.sh" "$VERSION" "$rid"; done ;;
  windows) "$ROOT_DIR/scripts/release/publish.sh" "$VERSION" win-x64; command -v pwsh >/dev/null || { echo "PowerShell 7 is required." >&2; exit 3; }; pwsh -NoProfile -File "$ROOT_DIR/scripts/release/package-windows.ps1" -Version "$VERSION" ;;
  finalize) python3 "$ROOT_DIR/scripts/release/finalize.py" "$VERSION" "$CHANNEL" "$REPOSITORY" ;;
  *) echo "Unknown target: $TARGET" >&2; exit 2;;
esac
