#!/usr/bin/env sh
set -eu
SCRIPT_DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
DEST="$HOME/.local/share/mosslight-farm/1.0"
APPS="$HOME/.local/share/applications"
ICON="$HOME/.local/share/icons/hicolor/scalable/apps"
mkdir -p "$DEST" "$APPS" "$ICON"
cp "$SCRIPT_DIR/index.html" "$SCRIPT_DIR/launcher.py" "$SCRIPT_DIR/run.sh" "$DEST/"
cp "$SCRIPT_DIR/mosslight-farm.svg" "$ICON/mosslight-farm.svg"
chmod 755 "$DEST/run.sh" "$DEST/launcher.py"
cat > "$APPS/mosslight-farm.desktop" <<EOF
[Desktop Entry]
Type=Application
Name=Mosslight Farm
Name[zh_CN]=苔光农场
Comment=An offline pixel-art desktop farm
Exec=$DEST/run.sh
Icon=mosslight-farm
Terminal=true
Categories=Game;Simulation;
EOF
chmod 644 "$APPS/mosslight-farm.desktop"
printf 'Installed Mosslight Farm 1.0. Launch it from your applications menu or run:\n%s\n' "$DEST/run.sh"
