#!/bin/bash
set -euo pipefail
cd "$(dirname "$0")"
if [[ "$(uname -s)" != Darwin ]]; then echo "This build needs a Mac with Xcode Command Line Tools."; exit 1; fi
if ! xcrun --find swiftc >/dev/null 2>&1; then echo "Install Xcode Command Line Tools first, then run this script again."; exit 1; fi
# A new output directory preserves previous app builds and all player saves.
output="$PWD/build/$(date +%Y%m%d-%H%M%S)"
app="$output/Mosslight Farm.app"
mkdir -p "$app/Contents/MacOS" "$app/Contents/Resources"
sdk="$(xcrun --sdk macosx --show-sdk-path)"
for arch in arm64 x86_64; do
  xcrun swiftc -swift-version 5 -O -sdk "$sdk" -target "$arch-apple-macosx13.0" \
    -framework AppKit -framework WebKit -framework ServiceManagement \
    Sources/Mosslight.swift -o "$output/Mosslight-$arch"
done
xcrun lipo -create "$output/Mosslight-arm64" "$output/Mosslight-x86_64" -output "$app/Contents/MacOS/Mosslight"
cp Info.plist "$app/Contents/Info.plist"
cp -R Resources/web "$app/Contents/Resources/web"
codesign --force --sign - "$app"
codesign --verify --strict "$app"
ditto -c -k --keepParent "$app" "$output/MosslightFarm-macOS-Universal-LocalBuild.zip"
echo "Built: $app"
echo "Copy the app to /Applications before enabling Start at login."
echo "Local build only: Developer ID signing and notarization are still required for public distribution."

