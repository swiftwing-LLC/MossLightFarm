# Mosslight Farm 1.1 — Windows source

This is the Windows interactive desktop-farm source, artwork, assets, and build scripts. Send friends the installer in `outputs/Windows/最新版`; do not send the game executable by itself.

Windows 10/11 64-bit and .NET Framework 4.8 are required. Run `build.ps1` to build the regular game and `打包/build-setup.ps1` to create the installer. The installer installs into the versioned `Applications/MosslightFarm/1.1` folder and preserves the regular save at `%USERPROFILE%/Saved Games/MosslightFarm/save.json`.

The installer registers `Mosslight Farm | 苔光农场` in Windows Settings → Apps → Installed apps. Its uninstaller removes the game, startup task, and shortcuts while preserving player saves. Rebuild the installer after changing either `Setup.cs` or `Uninstaller.cs`.

The seed page is a compact action panel: choose a seed, harvest ready crops, plant empty fields, or buy more land. It has no plot grid. All fields remain on the wallpaper and can still be moved directly there. Construction pauses farm production until the building finishes.

Run `打包/build-maintenance.ps1` to create the separate personal maintenance bundle. It starts in a game window, has unlimited coins, and gives every building and upgrade a 60-second timer. Its save lives at `%USERPROFILE%/Saved Games/MosslightFarm-Maintenance/`, with separate process and startup identifiers. It does not replace or migrate the normal farm save.

Version 1.0 remains the initial stable release. Windows follow-up releases increment the final digit (1.1, 1.2, and so on). Linux and macOS packages are separate platform builds.
