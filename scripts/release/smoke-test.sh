#!/usr/bin/env bash
set -euo pipefail
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
VERSION="${1:?Usage: smoke-test.sh <version>}"
CHANNEL="${WIISTUDIO_CHANNEL:-stable}"
RELEASE="$ROOT_DIR/artifacts/release/$VERSION"
python3 - "$RELEASE" "$VERSION" "$CHANNEL" <<'PY'
import hashlib, json, pathlib, sys, zipfile
release, version, channel = pathlib.Path(sys.argv[1]), sys.argv[2], sys.argv[3]
manifest = json.loads((release / "manifests" / f"wiistudio-update-manifest.{channel}.json").read_text())
assert manifest["latestVersion"] == version and len(manifest["packages"]) == 8
assert manifest["artifacts"] == manifest["packages"]
for item in manifest["packages"]:
    matches = list(release.glob(f"*/{item['fileName']}")); assert len(matches) == 1, item["fileName"]
    data = matches[0].read_bytes(); assert hashlib.sha256(data).hexdigest() == item["sha256"] and len(data) == item["sizeBytes"]
with zipfile.ZipFile(release / "windows" / f"WiiStudio-{version}-win-x64.zip") as archive:
    assert "WiiStudio.exe" in archive.namelist()
    assert "LICENSE.txt" in archive.namelist()
    assert "THIRD_PARTY_NOTICES.md" in archive.namelist()
for arch in ("x64", "arm64"):
    with zipfile.ZipFile(release / "macos" / f"WiiStudio-{version}-macos-{arch}.app.zip") as archive:
        names = archive.namelist(); assert any(n.endswith("WiiStudio.app/Contents/MacOS/WiiStudio") for n in names); assert any(n.endswith("WiiStudio.app/Contents/Info.plist") for n in names)
        assert any(n.endswith("WiiStudio.app/Contents/MacOS/LICENSE.txt") for n in names)
        assert any(n.endswith("WiiStudio.app/Contents/MacOS/THIRD_PARTY_NOTICES.md") for n in names)
sums = release / "checksums" / "SHA256SUMS.txt"
for line in sums.read_text().splitlines():
    digest, relative = line.split("  ", 1); assert hashlib.sha256((release / relative).read_bytes()).hexdigest() == digest
for forbidden in (".iso", ".wbfs", ".rom", ".bios", ".key"):
    assert not any(p.name.lower().endswith(forbidden) for p in release.rglob("*"))
PY
echo "Release smoke test passed: $RELEASE"
