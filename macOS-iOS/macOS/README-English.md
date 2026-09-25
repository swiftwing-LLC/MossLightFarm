# Mosslight Farm for macOS — 0.8 development source

This source has not caught up with Windows 0.12. It has not been compiled or played on a Mac, and there is no distributable `.app` or `.dmg`. Windows 0.12 uses Version 3 saves, which this older Mac build cannot read.

This is a native port's source package, **not a tested or ready-to-install Mac release**. It contains the Swift/AppKit/WKWebView host, local game simulation, pixel assets, and a universal build script. There is no Windows or Wallpaper Engine dependency.

On a Mac with macOS 13+ and Xcode Command Line Tools, run `bash build.sh` here. The script produces an Apple Silicon + Intel application in a new timestamped `build` folder and applies an ad-hoc local signature. Copy the resulting app to `/Applications` before enabling login startup. Public distribution still needs Developer ID signing, notarization, and real Mac testing.

Use the menu-bar clover for settings, window/desktop mode, display selection, saves and quit. Saves are stored in `~/Library/Application Support/MosslightFarm/`. This source uses Version 1 saves; do not transfer a Windows 0.12 save to it. There is no cloud sync.

Implemented: farming, processing queues, orders, buildings, expansion, animals, pets, decoration dragging, offline progress, English/Chinese and interface scaling. Only one display is active at a time.

33 simulation and Chromium browser checks passed on Windows. **Swift compilation, WebKit rendering, login startup, Retina, Spaces and Finder click-through have not been tested on a Mac.** Do not treat browser tests as native macOS acceptance.
