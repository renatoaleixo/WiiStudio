#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PROJECT_PATH="$ROOT_DIR/src/WiiStudio.App/WiiStudio.App.csproj"
WINDOWS_ICON_PATH="$ROOT_DIR/src/WiiStudio.App/Assets/wiistudio.ico"
MAC_ICON_SOURCE_PATH="$ROOT_DIR/src/WiiStudio.App/Assets/wiistudio-icon.png"
APP_ASSEMBLY_NAME="WiiStudio.App"
APP_DISPLAY_NAME="WiiStudio"
MAC_BUNDLE_IDENTIFIER="app.wiistudio.desktop"
VERSION="${1:-${WIISTUDIO_VERSION:-}}"
CHANNEL="${WIISTUDIO_CHANNEL:-stable}"
REPOSITORY="${WIISTUDIO_REPOSITORY:-renatoaleixo/WiiStudio}"
OUTPUT_ROOT="$ROOT_DIR/artifacts/release/${VERSION:-unversioned}"

if [[ -z "$VERSION" ]]; then
  echo "Usage: scripts/release/package.sh <semver-version>" >&2
  exit 2
fi

if [[ ! "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+([-+][0-9A-Za-z.-]+)?$ ]]; then
  echo "Version must be SemVer, for example 1.0.0 or 1.0.0-beta.1." >&2
  exit 2
fi

if [[ "$CHANNEL" != "stable" && "$CHANNEL" != "beta" && "$CHANNEL" != "dev" ]]; then
  echo "WIISTUDIO_CHANNEL must be stable, beta or dev." >&2
  exit 2
fi

rm -rf "$OUTPUT_ROOT"
mkdir -p "$OUTPUT_ROOT/publish" "$OUTPUT_ROOT/package"

publish_runtime() {
  local rid="$1"
  local publish_dir="$OUTPUT_ROOT/publish/$rid"

  dotnet publish "$PROJECT_PATH" \
    -c Release \
    -r "$rid" \
    --self-contained true \
    -p:PublishSingleFile=false \
    -p:PublishTrimmed=false \
    -p:Version="$VERSION" \
    -o "$publish_dir"
}

copy_publish_to_package() {
  local rid="$1"
  local publish_dir="$OUTPUT_ROOT/publish/$rid"
  local package_dir="$OUTPUT_ROOT/package/$rid"

  rm -rf "$package_dir"
  mkdir -p "$package_dir"
  cp -a "$publish_dir"/. "$package_dir"/
}

prepare_windows_package() {
  local rid="win-x64"
  local package_dir="$OUTPUT_ROOT/package/$rid"
  local source_exe="$package_dir/$APP_ASSEMBLY_NAME.exe"
  local target_exe="$package_dir/$APP_DISPLAY_NAME.exe"

  copy_publish_to_package "$rid"

  if [[ ! -f "$WINDOWS_ICON_PATH" ]]; then
    echo "Missing Windows icon: $WINDOWS_ICON_PATH" >&2
    exit 1
  fi

  if [[ ! -f "$source_exe" ]]; then
    echo "Missing Windows executable: $source_exe" >&2
    exit 1
  fi

  cp "$source_exe" "$target_exe"
  rm "$source_exe"
}

create_macos_icns() {
  local destination="$1"

  if [[ ! -f "$MAC_ICON_SOURCE_PATH" ]]; then
    echo "Missing macOS icon source: $MAC_ICON_SOURCE_PATH" >&2
    exit 1
  fi

  python3 - "$MAC_ICON_SOURCE_PATH" "$destination" <<'PY'
import pathlib
import struct
import sys

source = pathlib.Path(sys.argv[1])
destination = pathlib.Path(sys.argv[2])
png = source.read_bytes()
if not png.startswith(b"\x89PNG\r\n\x1a\n"):
    raise SystemExit(f"macOS icon source is not a PNG: {source}")

# ICNS supports PNG payloads in modern icon entries. The ic10 entry carries
# the 1024x1024 representation used by Finder/Dock on current macOS releases.
entry_type = b"ic10"
entry_size = 8 + len(png)
total_size = 8 + entry_size
destination.write_bytes(
    b"icns"
    + struct.pack(">I", total_size)
    + entry_type
    + struct.pack(">I", entry_size)
    + png)
PY
}

prepare_macos_package() {
  local rid="$1"
  local publish_dir="$OUTPUT_ROOT/publish/$rid"
  local package_dir="$OUTPUT_ROOT/package/$rid"
  local bundle_dir="$package_dir/$APP_DISPLAY_NAME.app"
  local macos_dir="$bundle_dir/Contents/MacOS"
  local resources_dir="$bundle_dir/Contents/Resources"
  local source_executable="$macos_dir/$APP_ASSEMBLY_NAME"
  local target_executable="$macos_dir/$APP_DISPLAY_NAME"

  rm -rf "$package_dir"
  mkdir -p "$macos_dir" "$resources_dir"
  cp -a "$publish_dir"/. "$macos_dir"/

  if [[ ! -f "$source_executable" ]]; then
    echo "Missing macOS executable: $source_executable" >&2
    exit 1
  fi

  cp "$source_executable" "$target_executable"
  rm "$source_executable"
  chmod +x "$target_executable"

  create_macos_icns "$resources_dir/$APP_DISPLAY_NAME.icns"

  cat > "$bundle_dir/Contents/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN"
 "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key>
  <string>$APP_DISPLAY_NAME</string>
  <key>CFBundleDisplayName</key>
  <string>$APP_DISPLAY_NAME</string>
  <key>CFBundleIdentifier</key>
  <string>$MAC_BUNDLE_IDENTIFIER</string>
  <key>CFBundleExecutable</key>
  <string>$APP_DISPLAY_NAME</string>
  <key>CFBundleIconFile</key>
  <string>$APP_DISPLAY_NAME</string>
  <key>CFBundleShortVersionString</key>
  <string>$VERSION</string>
  <key>CFBundleVersion</key>
  <string>$VERSION</string>
  <key>LSMinimumSystemVersion</key>
  <string>12.0</string>
  <key>NSHighResolutionCapable</key>
  <true/>
</dict>
</plist>
PLIST
}

prepare_packages() {
  copy_publish_to_package linux-x64
  prepare_windows_package
  prepare_macos_package osx-x64
  prepare_macos_package osx-arm64
}

archive_runtime() {
  local rid="$1"
  local kind="$2"
  local package_dir="$OUTPUT_ROOT/package/$rid"
  local artifact_name

  if [[ "$kind" == "zip" ]]; then
    artifact_name="WiiStudio-${VERSION}-${rid}.zip"
    python3 - "$package_dir" "$OUTPUT_ROOT/$artifact_name" <<'PY'
import pathlib
import sys
import zipfile

source = pathlib.Path(sys.argv[1])
destination = pathlib.Path(sys.argv[2])
with zipfile.ZipFile(destination, "w", compression=zipfile.ZIP_DEFLATED) as archive:
    for path in sorted(source.rglob("*")):
        if path.is_file():
            archive.write(path, path.relative_to(source))
PY
  else
    artifact_name="WiiStudio-${VERSION}-${rid}.tar.gz"
    tar -C "$package_dir" -czf "$OUTPUT_ROOT/$artifact_name" .
  fi
}

rids=(linux-x64 win-x64 osx-x64 osx-arm64)
for rid in "${rids[@]}"; do
  publish_runtime "$rid"
done

prepare_packages

archive_runtime linux-x64 tar
archive_runtime win-x64 zip
archive_runtime osx-x64 tar
archive_runtime osx-arm64 tar

(
  cd "$OUTPUT_ROOT"
  sha256sum WiiStudio-* > SHA256SUMS.txt
)

python3 - "$OUTPUT_ROOT" "$VERSION" "$CHANNEL" "$REPOSITORY" <<'PY'
import datetime
import hashlib
import json
import pathlib
import sys

output_root = pathlib.Path(sys.argv[1])
version = sys.argv[2]
channel = sys.argv[3]
repository = sys.argv[4]
tag = f"v{version}"
release_url = f"https://github.com/{repository}/releases/download/{tag}"
release_notes_url = f"https://github.com/{repository}/releases/tag/{tag}"

packages = []
for artifact in sorted(output_root.glob("WiiStudio-*")):
    if not artifact.is_file():
        continue

    name = artifact.name
    if name.endswith(".tar.gz"):
        platform = name.removeprefix(f"WiiStudio-{version}-").removesuffix(".tar.gz")
        kind = "tar.gz"
    elif name.endswith(".zip"):
        platform = name.removeprefix(f"WiiStudio-{version}-").removesuffix(".zip")
        kind = "zip"
    else:
        continue

    package = {
        "platform": platform,
        "kind": kind,
        "fileName": name,
        "url": f"{release_url}/{name}",
        "sha256": hashlib.sha256(artifact.read_bytes()).hexdigest(),
        "signatureUrl": f"{release_url}/{name}.sig",
        "signatureAlgorithm": "pending",
        "signingKeyId": "pending",
        "sizeBytes": artifact.stat().st_size
    }
    if platform == "win-x64":
        package["mainExecutable"] = "WiiStudio.exe"
    elif platform.startswith("osx-"):
        package["bundleName"] = "WiiStudio.app"
        package["bundleIdentifier"] = "app.wiistudio.desktop"

    packages.append(package)

manifest = {
    "schemaVersion": 1,
    "applicationId": "com.wiistudio.app",
    "channel": channel,
    "latestVersion": version,
    "minimumSupportedVersion": "0.1.0",
    "publishedAt": datetime.datetime.now(datetime.UTC).isoformat().replace("+00:00", "Z"),
    "releaseNotesUrl": release_notes_url,
    "packages": packages
}

(output_root / f"wiistudio-update-manifest.{channel}.json").write_text(
    json.dumps(manifest, indent=2, ensure_ascii=False) + "\n",
    encoding="utf-8")
PY

echo "Release artifacts generated in $OUTPUT_ROOT"
