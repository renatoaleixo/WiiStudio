#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
VERSION="${1:?Usage: package-linux.sh <version>}"
PUBLISH="$ROOT_DIR/artifacts/publish/$VERSION/linux-x64"
RELEASE="$ROOT_DIR/artifacts/release/$VERSION/linux"
APPDIR="$ROOT_DIR/artifacts/work/$VERSION/WiiStudio.AppDir"
APPIMAGETOOL="${APPIMAGETOOL:-appimagetool}"

[[ -x "$PUBLISH/WiiStudio" ]] || { echo "Run publish.sh $VERSION linux-x64 first." >&2; exit 2; }
rm -rf "$RELEASE" "$APPDIR"
mkdir -p "$RELEASE" "$APPDIR/usr/lib/wiistudio" "$APPDIR/usr/bin" "$APPDIR/usr/share/applications" \
  "$APPDIR/usr/share/icons/hicolor/512x512/apps"
cp -a "$PUBLISH"/. "$APPDIR/usr/lib/wiistudio/"
ln -s ../lib/wiistudio/WiiStudio "$APPDIR/usr/bin/WiiStudio"
cp "$ROOT_DIR/packaging/linux/wiistudio.desktop" "$APPDIR/wiistudio.desktop"
cp "$ROOT_DIR/packaging/linux/wiistudio.desktop" "$APPDIR/usr/share/applications/wiistudio.desktop"
cp "$ROOT_DIR/src/WiiStudio.App/Assets/wiistudio-icon.png" "$APPDIR/wiistudio.png"
cp "$ROOT_DIR/src/WiiStudio.App/Assets/wiistudio-icon.png" "$APPDIR/usr/share/icons/hicolor/512x512/apps/wiistudio.png"
cat > "$APPDIR/AppRun" <<'EOF'
#!/usr/bin/env bash
HERE="$(dirname "$(readlink -f "${0}")")"
exec "$HERE/usr/lib/wiistudio/WiiStudio" "$@"
EOF
chmod +x "$APPDIR/AppRun"

command -v "$APPIMAGETOOL" >/dev/null 2>&1 || { echo "appimagetool is required (set APPIMAGETOOL)." >&2; exit 3; }
ARCH=x86_64 "$APPIMAGETOOL" "$APPDIR" "$RELEASE/WiiStudio-$VERSION-linux-x64.AppImage"
chmod +x "$RELEASE/WiiStudio-$VERSION-linux-x64.AppImage"

DEBROOT="$ROOT_DIR/artifacts/work/$VERSION/deb"
rm -rf "$DEBROOT"
mkdir -p "$DEBROOT/DEBIAN" "$DEBROOT/opt/wiistudio" "$DEBROOT/usr/bin" \
  "$DEBROOT/usr/share/applications" "$DEBROOT/usr/share/icons/hicolor/512x512/apps" \
  "$DEBROOT/usr/share/polkit-1/actions"
cp -a "$PUBLISH"/. "$DEBROOT/opt/wiistudio/"
ln -s /opt/wiistudio/WiiStudio "$DEBROOT/usr/bin/wiistudio"
sed 's/Exec=WiiStudio/Exec=wiistudio/' "$ROOT_DIR/packaging/linux/wiistudio.desktop" > "$DEBROOT/usr/share/applications/wiistudio.desktop"
cp "$ROOT_DIR/src/WiiStudio.App/Assets/wiistudio-icon.png" "$DEBROOT/usr/share/icons/hicolor/512x512/apps/wiistudio.png"
cp "$ROOT_DIR/packaging/linux/com.wiistudio.device.policy" "$DEBROOT/usr/share/polkit-1/actions/"
cat > "$DEBROOT/DEBIAN/control" <<EOF
Package: wiistudio
Version: $VERSION
Section: games
Priority: optional
Architecture: amd64
Maintainer: WiiStudio
Description: Cross-platform Nintendo Wii game manager
EOF
dpkg-deb --root-owner-group --build "$DEBROOT" "$RELEASE/wiistudio_${VERSION}_amd64.deb"
