#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
VERSION="${1:?Usage: package-macos.sh <version> <runtime>}"
RID="${2:?Usage: package-macos.sh <version> <runtime>}"
[[ "$RID" == osx-x64 || "$RID" == osx-arm64 ]] || { echo "Invalid macOS runtime: $RID" >&2; exit 2; }
PUBLISH="$ROOT_DIR/artifacts/publish/$VERSION/$RID"
RELEASE="$ROOT_DIR/artifacts/release/$VERSION/macos"
WORK="$ROOT_DIR/artifacts/work/$VERSION/$RID"
APP="$WORK/WiiStudio.app"
[[ -x "$PUBLISH/WiiStudio" ]] || { echo "Run publish.sh $VERSION $RID first." >&2; exit 2; }
command -v hdiutil >/dev/null || { echo "hdiutil is required; run this package on macOS." >&2; exit 3; }

rm -rf "$WORK"
mkdir -p "$RELEASE" "$APP/Contents/MacOS" "$APP/Contents/Resources"
cp -a "$PUBLISH"/. "$APP/Contents/MacOS/"
sed "s/@VERSION@/$VERSION/g" "$ROOT_DIR/packaging/macos/Info.plist.in" > "$APP/Contents/Info.plist"

ICONSET="$WORK/WiiStudio.iconset"
mkdir -p "$ICONSET"
for spec in '16 icon_16x16.png' '32 icon_16x16@2x.png' '32 icon_32x32.png' '64 icon_32x32@2x.png' '128 icon_128x128.png' '256 icon_128x128@2x.png' '256 icon_256x256.png' '512 icon_256x256@2x.png' '512 icon_512x512.png' '1024 icon_512x512@2x.png'; do
  set -- $spec; sips -z "$1" "$1" "$ROOT_DIR/src/WiiStudio.App/Assets/wiistudio-icon.png" --out "$ICONSET/$2" >/dev/null
done
iconutil -c icns "$ICONSET" -o "$APP/Contents/Resources/WiiStudio.icns"

ditto -c -k --sequesterRsrc --keepParent "$APP" "$RELEASE/WiiStudio-$VERSION-macos-${RID#osx-}.app.zip"
DMGROOT="$WORK/dmg"
mkdir -p "$DMGROOT"
cp -a "$APP" "$DMGROOT/"
ln -s /Applications "$DMGROOT/Applications"
hdiutil create -volname WiiStudio -srcfolder "$DMGROOT" -ov -format UDZO "$RELEASE/WiiStudio-$VERSION-macos-${RID#osx-}.dmg"
