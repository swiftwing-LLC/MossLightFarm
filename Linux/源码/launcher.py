#!/usr/bin/env python3
"""Launch Mosslight Farm as a local, offline browser app on Linux."""
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
import shutil
import subprocess
import sys
import time
import webbrowser

ROOT = Path(__file__).resolve().parent
PORTS = range(41730, 41740)


def open_game(url: str) -> subprocess.Popen | None:
    profile_root = Path.home() / ".local" / "share" / "mosslight-farm" / "browser-profile"
    profile_root.mkdir(parents=True, exist_ok=True)
    for name in ("chromium", "chromium-browser", "google-chrome", "brave-browser", "microsoft-edge"):
        browser = shutil.which(name)
        if browser:
            return subprocess.Popen([browser, f"--user-data-dir={profile_root / 'chromium'}", "--no-first-run", f"--app={url}"], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    for name in ("firefox", "firefox-esr"):
        browser = shutil.which(name)
        if browser:
            return subprocess.Popen([browser, "-profile", str(profile_root / "firefox"), "--new-window", url], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
    if not webbrowser.open(url, new=1):
        raise RuntimeError("No supported browser was found. Install Chromium, Firefox, or another desktop browser.")
    return None


def main() -> int:
    handler = lambda *args, **kwargs: SimpleHTTPRequestHandler(*args, directory=str(ROOT), **kwargs)
    server = None
    for port in PORTS:
        try:
            server = ThreadingHTTPServer(("127.0.0.1", port), handler)
            break
        except OSError:
            continue
    if server is None:
        print("Mosslight Farm could not find an available local port (41730–41739).", file=sys.stderr)
        return 1
    server.daemon_threads = True
    server.timeout = 0.4
    url = f"http://127.0.0.1:{server.server_port}/index.html"
    print(f"Mosslight Farm 1.0 is running locally at {url}")
    print("Keep this launcher running while you play; press Ctrl+C here to stop the local server.")
    process = None
    try:
        process = open_game(url)
        while process is None or process.poll() is None:
            server.handle_request()
    except KeyboardInterrupt:
        pass
    except Exception as error:
        print(f"Could not open the game: {error}", file=sys.stderr)
        return 1
    finally:
        server.server_close()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
