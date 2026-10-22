#!/usr/bin/env python3
import datetime, hashlib, json, pathlib, re, sys

root = pathlib.Path(__file__).resolve().parents[2]
version = sys.argv[1] if len(sys.argv) > 1 else ""
channel = sys.argv[2] if len(sys.argv) > 2 else "stable"
repository = sys.argv[3] if len(sys.argv) > 3 else "renatoaleixo/WiiStudio"
if not re.fullmatch(r"[0-9]+\.[0-9]+\.[0-9]+(?:[-+][0-9A-Za-z.-]+)?", version):
    raise SystemExit("Usage: finalize.py <semver-version> [stable|beta|dev] [owner/repository]")
if channel not in {"stable", "beta", "dev"}: raise SystemExit("Invalid release channel")
release = root / "artifacts" / "release" / version
specs = {"windows": [(f"WiiStudioSetup-{version}-win-x64.exe", "win-x64", "inno-setup"), (f"WiiStudio-{version}-win-x64.zip", "win-x64", "zip")],
         "linux": [(f"WiiStudio-{version}-linux-x64.AppImage", "linux-x64", "appimage"), (f"wiistudio_{version}_amd64.deb", "linux-x64", "deb")],
         "macos": [(f"WiiStudio-{version}-macos-x64.dmg", "osx-x64", "dmg"), (f"WiiStudio-{version}-macos-x64.app.zip", "osx-x64", "app-zip"), (f"WiiStudio-{version}-macos-arm64.dmg", "osx-arm64", "dmg"), (f"WiiStudio-{version}-macos-arm64.app.zip", "osx-arm64", "app-zip")]}
artifacts = []
for folder, entries in specs.items():
    for filename, platform, kind in entries:
        path = release / folder / filename
        if not path.is_file() or path.stat().st_size == 0: raise SystemExit(f"Missing release artifact: {path}")
        artifacts.append({"platform": platform, "kind": kind, "fileName": filename,
                          "url": f"https://github.com/{repository}/releases/download/v{version}/{filename}",
                          "sha256": hashlib.sha256(path.read_bytes()).hexdigest(), "sizeBytes": path.stat().st_size})
manifests, checksums = release / "manifests", release / "checksums"
manifests.mkdir(parents=True, exist_ok=True); checksums.mkdir(parents=True, exist_ok=True)
manifest = {"schemaVersion": 1, "applicationId": "com.wiistudio.app", "channel": channel, "latestVersion": version,
            "version": version, "minimumSupportedVersion": "0.1.0",
            "publishedAt": datetime.datetime.now(datetime.UTC).isoformat().replace("+00:00", "Z"),
            "releaseNotesUrl": f"https://github.com/{repository}/releases/tag/v{version}",
            "packages": artifacts, "artifacts": artifacts}
(manifests / f"wiistudio-update-manifest.{channel}.json").write_text(json.dumps(manifest, indent=2) + "\n")
files = [p for p in release.rglob("*") if p.is_file() and "checksums" not in p.parts]
(checksums / "SHA256SUMS.txt").write_text("\n".join(f"{hashlib.sha256(p.read_bytes()).hexdigest()}  {p.relative_to(release).as_posix()}" for p in sorted(files)) + "\n")
