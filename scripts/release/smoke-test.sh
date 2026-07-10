#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
VERSION="${1:-${WIISTUDIO_VERSION:-}}"
CHANNEL="${WIISTUDIO_CHANNEL:-stable}"
RELEASE_DIR="$ROOT_DIR/artifacts/release/${VERSION:-unversioned}"

if [[ -z "$VERSION" ]]; then
  echo "Usage: scripts/release/smoke-test.sh <semver-version>" >&2
  exit 2
fi

required=(
  "WiiStudio-${VERSION}-linux-x64.tar.gz"
  "WiiStudio-${VERSION}-win-x64.zip"
  "WiiStudio-${VERSION}-osx-x64.tar.gz"
  "WiiStudio-${VERSION}-osx-arm64.tar.gz"
  "SHA256SUMS.txt"
  "wiistudio-update-manifest.${CHANNEL}.json"
)

for file in "${required[@]}"; do
  if [[ ! -s "$RELEASE_DIR/$file" ]]; then
    echo "Missing release artifact: $file" >&2
    exit 1
  fi
done

(
  cd "$RELEASE_DIR"
  sha256sum --check SHA256SUMS.txt
)

python3 - "$RELEASE_DIR/wiistudio-update-manifest.${CHANNEL}.json" "$VERSION" "$CHANNEL" <<'PY'
import json
import pathlib
import sys

manifest_path = pathlib.Path(sys.argv[1])
version = sys.argv[2]
channel = sys.argv[3]
manifest = json.loads(manifest_path.read_text(encoding="utf-8"))

assert manifest["schemaVersion"] == 1
assert manifest["applicationId"] == "com.wiistudio.app"
assert manifest["channel"] == channel
assert manifest["latestVersion"] == version
assert len(manifest["packages"]) == 4

for package in manifest["packages"]:
    artifact = manifest_path.parent / package["fileName"]
    assert artifact.is_file(), package["fileName"]
    assert package["sizeBytes"] == artifact.stat().st_size
    assert package["url"].startswith("https://github.com/")
    assert len(package["sha256"]) == 64
PY

echo "Release smoke test passed for $RELEASE_DIR"
