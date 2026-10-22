#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
VERSION="${1:?Usage: publish.sh <version> <runtime>}"
RID="${2:?Usage: publish.sh <version> <runtime>}"
OUTPUT="$ROOT_DIR/artifacts/publish/$VERSION/$RID"

case "$RID" in win-x64|linux-x64|osx-x64|osx-arm64) ;; *) echo "Unsupported runtime: $RID" >&2; exit 2;; esac
[[ "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+([-+][0-9A-Za-z.-]+)?$ ]] || { echo "Invalid SemVer: $VERSION" >&2; exit 2; }

rm -rf "$OUTPUT"
dotnet publish "$ROOT_DIR/src/WiiStudio.App/WiiStudio.App.csproj" -c Release -r "$RID" \
  --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=false \
  -p:Version="$VERSION" -o "$OUTPUT"

if [[ "$RID" == win-* ]]; then
  mv "$OUTPUT/WiiStudio.App.exe" "$OUTPUT/WiiStudio.exe"
else
  mv "$OUTPUT/WiiStudio.App" "$OUTPUT/WiiStudio"
  chmod +x "$OUTPUT/WiiStudio"
fi
