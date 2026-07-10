#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
PROJECT_PATH="$ROOT_DIR/src/WiiStudio.App/WiiStudio.App.csproj"
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
mkdir -p "$OUTPUT_ROOT/publish"

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

archive_runtime() {
  local rid="$1"
  local kind="$2"
  local publish_dir="$OUTPUT_ROOT/publish/$rid"
  local artifact_name

  if [[ "$kind" == "zip" ]]; then
    artifact_name="WiiStudio-${VERSION}-${rid}.zip"
    python3 - "$publish_dir" "$OUTPUT_ROOT/$artifact_name" <<'PY'
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
    tar -C "$publish_dir" -czf "$OUTPUT_ROOT/$artifact_name" .
  fi
}

rids=(linux-x64 win-x64 osx-x64 osx-arm64)
for rid in "${rids[@]}"; do
  publish_runtime "$rid"
done

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

    packages.append({
        "platform": platform,
        "kind": kind,
        "fileName": name,
        "url": f"{release_url}/{name}",
        "sha256": hashlib.sha256(artifact.read_bytes()).hexdigest(),
        "signatureUrl": f"{release_url}/{name}.sig",
        "signatureAlgorithm": "pending",
        "signingKeyId": "pending",
        "sizeBytes": artifact.stat().st_size
    })

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
