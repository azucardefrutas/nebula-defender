"""Build the same game using the installed Web-capable Editor, preserving the Unity 6.3 source project."""
from pathlib import Path
import json
import shutil
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / "BuildWork/UnityWeb"
EDITOR = Path(r"C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe")


def main():
    if not EDITOR.exists():
        raise RuntimeError("Unity 6000.4.6f1 with Web Build Support is required for this isolated web export.")
    WORK.mkdir(parents=True, exist_ok=True)
    for name in ["Assets", "Packages", "ProjectSettings"]:
        shutil.copytree(ROOT / name, WORK / name, dirs_exist_ok=True)
    manifest_path = WORK / "Packages/manifest.json"
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    manifest["dependencies"]["com.unity.render-pipelines.universal"] = "17.4.0"
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    (WORK / "ProjectSettings/ProjectVersion.txt").write_text("m_EditorVersion: 6000.4.6f1\nm_EditorVersionWithRevision: 6000.4.6f1 (0b051c2e5d54)\n", encoding="utf-8")
    evidence = ROOT / "Documentation/Evidence"
    command = [str(EDITOR), "-batchmode", "-quit", "-projectPath", str(WORK), "-buildTarget", "WebGL", "-executeMethod", "NebulaDefender.Editor.WebBuilder.BuildWeb", "-logFile", str(evidence / "web-build.log"), "--burst-disable-compilation"]
    print("Building the same game for Web in an isolated copy. Original Unity 6.3 project is preserved.", flush=True)
    result = subprocess.run(command, cwd=WORK)
    summary = WORK / "Documentation/Evidence/web-build-summary.txt"
    if summary.exists():
        shutil.copy2(summary, evidence / summary.name)
    if result.returncode:
        return result.returncode
    output = WORK / "Builds/WebGL"
    if not (output / "index.html").is_file() or not list((output / "Build").glob("*.wasm.gz")):
        raise RuntimeError("Web build did not produce the expected HTML and WebAssembly files.")
    shutil.copytree(output, ROOT / "Builds/WebGL", dirs_exist_ok=True)
    print("WEB_BUILD_READY: Builds/WebGL/index.html", flush=True)
    return 0


if __name__ == "__main__":
    sys.exit(main())
