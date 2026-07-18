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
  "release-validation/version.txt"
  "release-validation/environment-linux.md"
  "release-validation/environment-windows.md"
  "release-validation/environment-macos.md"
  "release-validation/hardware-matrix.md"
  "release-validation/release-checklist.md"
  "release-validation/acceptance-report.md"
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
  grep -F "wiistudio-update-manifest.${CHANNEL}.json" SHA256SUMS.txt >/dev/null
  grep -F "release-validation/acceptance-report.md" SHA256SUMS.txt >/dev/null
)

python3 - "$RELEASE_DIR/wiistudio-update-manifest.${CHANNEL}.json" "$VERSION" "$CHANNEL" <<'PY'
import json
import pathlib
import struct
import sys
import tarfile
import zipfile

manifest_path = pathlib.Path(sys.argv[1])
version = sys.argv[2]
channel = sys.argv[3]
release_dir = manifest_path.parent
manifest = json.loads(manifest_path.read_text(encoding="utf-8"))

assert manifest["schemaVersion"] == 1
assert manifest["applicationId"] == "com.wiistudio.app"
assert manifest["channel"] == channel
assert manifest["latestVersion"] == version
assert len(manifest["packages"]) == 4

def has_windows_icon_resources(executable: bytes) -> bool:
    pe_offset = struct.unpack_from("<I", executable, 0x3C)[0]
    if executable[pe_offset:pe_offset + 4] != b"PE\0\0":
        return False

    coff = pe_offset + 4
    section_count = struct.unpack_from("<H", executable, coff + 2)[0]
    optional_header_size = struct.unpack_from("<H", executable, coff + 16)[0]
    section_offset = coff + 20 + optional_header_size
    resource_section = None

    for index in range(section_count):
        offset = section_offset + index * 40
        name = executable[offset:offset + 8].split(b"\0", 1)[0]
        virtual_address, raw_pointer = struct.unpack_from("<II", executable, offset + 12)[0], struct.unpack_from("<I", executable, offset + 20)[0]
        if name == b".rsrc":
            resource_section = (virtual_address, raw_pointer)
            break

    if resource_section is None:
        return False

    _, resource_offset = resource_section
    named_count = struct.unpack_from("<H", executable, resource_offset + 12)[0]
    id_count = struct.unpack_from("<H", executable, resource_offset + 14)[0]
    resource_types = set()
    entry_offset = resource_offset + 16

    for index in range(named_count + id_count):
        offset = entry_offset + index * 8
        raw_name = struct.unpack_from("<I", executable, offset)[0]
        if raw_name & 0x80000000:
            continue
        resource_types.add(raw_name)

    return 3 in resource_types and 14 in resource_types

def normalize_tar_name(name: str) -> str:
    return name[2:] if name.startswith("./") else name

def read_tar_member(archive: tarfile.TarFile, normalized_name: str) -> bytes:
    for member in archive.getmembers():
        if normalize_tar_name(member.name) == normalized_name:
            file = archive.extractfile(member)
            assert file is not None
            return file.read()
    raise AssertionError(f"Missing tar member: {normalized_name}")

def validate_windows_package(artifact: pathlib.Path, package: dict) -> None:
    assert package.get("mainExecutable") == "WiiStudio.exe"
    with zipfile.ZipFile(artifact) as archive:
        names = set(archive.namelist())
        assert "WiiStudio.exe" in names
        assert "WiiStudio.App.exe" not in names
        assert "WiiStudio.App.dll" in names
        assert "WiiStudio.App.deps.json" in names
        assert "WiiStudio.App.runtimeconfig.json" in names
        assert has_windows_icon_resources(archive.read("WiiStudio.exe"))

def validate_macos_package(artifact: pathlib.Path, package: dict) -> None:
    assert package.get("bundleName") == "WiiStudio.app"
    assert package.get("bundleIdentifier") == "app.wiistudio.desktop"
    with tarfile.open(artifact, "r:gz") as archive:
        names = {normalize_tar_name(member.name) for member in archive.getmembers()}
        executable = "WiiStudio.app/Contents/MacOS/WiiStudio"
        plist = "WiiStudio.app/Contents/Info.plist"
        icon = "WiiStudio.app/Contents/Resources/WiiStudio.icns"
        assert executable in names
        assert plist in names
        assert icon in names
        assert "WiiStudio.app/Contents/MacOS/WiiStudio.App" not in names

        plist_text = read_tar_member(archive, plist).decode("utf-8")
        assert "<key>CFBundleName</key>" in plist_text
        assert "<string>WiiStudio</string>" in plist_text
        assert "<key>CFBundleExecutable</key>" in plist_text
        assert "<key>CFBundleIconFile</key>" in plist_text

        assert read_tar_member(archive, icon).startswith(b"icns")

for package in manifest["packages"]:
    artifact = release_dir / package["fileName"]
    assert artifact.is_file(), package["fileName"]
    assert package["sizeBytes"] == artifact.stat().st_size
    assert package["url"].startswith("https://github.com/")
    assert len(package["sha256"]) == 64

    if package["platform"] == "win-x64":
        validate_windows_package(artifact, package)
    elif package["platform"].startswith("osx-"):
        validate_macos_package(artifact, package)
PY

echo "Release smoke test passed for $RELEASE_DIR"
