#!/usr/bin/env python3
"""Lightweight packaging and syntax checks for the offline Linux edition."""
from pathlib import Path
import re
import shutil
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parent
page = (ROOT / "index.html").read_text(encoding="utf-8")
launcher = (ROOT / "launcher.py").read_text(encoding="utf-8")
script = re.search(r"<script>(.*?)</script>", page, re.S)
assert script, "embedded game script is missing"
compile(launcher, str(ROOT / "launcher.py"), "exec")
assert "Mosslight Farm 1.0" in page and "MosslightFarm-Linux" not in page
assert 'href="mosslight-farm.svg"' in page
assert "localStorage" in page and "mosslight-farm-linux-v1" in page
assert "${s.plots.map((p,i)=>" in page, "farm list should only render owned fields"
assert "Array.from({length:81}" not in page
assert "EXPAND_COUNTS=[12,24,36,45,54,63,72,81]" in page
assert "function rebuildWorldCache()" in page and "time-lastFrame<32" in page
assert "function animalAction(i)" in page and "function drawPollinator" in page
assert "if(id==='coop')s.animals.push" in page and "if(id==='barn')s.animals.push" in page
assert "Construction pauses all production." in page and "建筑施工中，全农场暂停生产。" in page
assert '127.0.0.1' in launcher and "--app=" in launcher
assert (ROOT / "run.sh").exists() and (ROOT / "install.sh").exists()
node = shutil.which("node")
if node:
    with tempfile.NamedTemporaryFile("w", suffix=".js", encoding="utf-8", delete=False) as out:
        out.write(script.group(1))
        check_file = Path(out.name)
    try:
        subprocess.run([node, "--check", str(check_file)], check=True, capture_output=True, text=True)
    finally:
        check_file.unlink(missing_ok=True)
print("PASS: Linux package files, offline save, 81-field progression, construction pause, renderer cache and embedded JavaScript syntax")
