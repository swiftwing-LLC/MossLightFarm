# Mosslight Farm 1.0 for Linux

This portable Linux edition runs the interactive farm in a local browser app window. It works offline after launch, needs Python 3 and a desktop browser, and stores progress in that browser's local storage. It does not require Wallpaper Engine.

## Run from the extracted folder

```sh
chmod +x run.sh
./run.sh
```

Chromium-family browsers open in app mode; Firefox opens a dedicated window. The game gets its own browser profile so saves stay separate from normal browsing. Keep the terminal open while playing, or press Ctrl+C there to stop the local server. On first use, `install.sh` can add the game to the desktop applications menu under `~/.local/share`.

The game includes Simplified Chinese and English, planting and harvesting, expansion through 81 fields, storage and orders, construction pauses, movable scenery, chickens and cows that roam and produce goods, pollinators, birds, and animated weather. Click a farm animal to feed it with wheat, then return when its product is ready. This Linux edition is a game window; desktop wallpaper integration varies across Linux desktop environments and is not enabled by this package.

No network connection is used by the game itself. Chromium and Firefox are launched with a game-only profile, and saves persist locally across sessions.
