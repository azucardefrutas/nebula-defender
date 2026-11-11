"""Assemble existing verified player files on a standard Windows GitHub runner.

This does not build Unity or replace gameplay binaries. It verifies every file
installed by the already-published installer before creating portable archives.
"""
from pathlib import Path
import hashlib
import io
import json
import os
import subprocess
import tempfile
import zipfile

ROOT = Path(__file__).resolve().parents[1]
REPO = "azucardefrutas/nebula-defender"
TAG = "v1.0.0"


def sha(path):
    result = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            result.update(block)
    return result.hexdigest()


def gh(*args, json_result=False):
    result = subprocess.run(["gh", *args], check=True, capture_output=True, text=True)
    return json.loads(result.stdout) if json_result else result.stdout


def assets():
    return gh("api", f"repos/{REPO}/releases/tags/{TAG}", json_result=True)["assets"]


def download(name, folder):
    gh("release", "download", TAG, "--repo", REPO, "--pattern", name, "--dir", str(folder))
    return folder / name


def verify_windows(archive, manifest):
    with zipfile.ZipFile(archive) as bundle:
        if bundle.testzip() is not None:
            raise RuntimeError("Portable archive CRC failed.")
        files = {n for n in bundle.namelist() if not n.endswith("/")}
        expected = {"Windows/" + name for name in manifest}
        if files != expected:
            raise RuntimeError("Portable file inventory differs from the verified player.")
        for name, expected_sha in manifest.items():
            if hashlib.sha256(bundle.read("Windows/" + name)).hexdigest() != expected_sha:
                raise RuntimeError("Portable file changed: " + name)


def upload_or_reuse(path, verify):
    current = next((a for a in assets() if a["name"] == path.name), None)
    if current and current["state"] == "uploaded":
        retained = download(path.name, path.parent / "existing")
        verify(retained)
        print("Retained and verified existing asset:", path.name, flush=True)
        return retained
    if current:
        if current["state"] != "starter" or current.get("digest"):
            raise RuntimeError("Refusing to replace an unknown release asset.")
        gh("api", f"repos/{REPO}/releases/assets/{current['id']}", "--method", "DELETE")
    gh("release", "upload", TAG, str(path), "--repo", REPO)
    remote = next(a for a in assets() if a["name"] == path.name)
    if remote["state"] != "uploaded" or remote["digest"] != "sha256:" + sha(path):
        raise RuntimeError("Release asset digest does not match: " + path.name)
    print("Uploaded and verified:", path.name, flush=True)
    return path


def main():
    if os.name != "nt" or os.environ.get("GITHUB_REPOSITORY") != REPO:
        raise RuntimeError("Run this workflow on the project's standard Windows GitHub runner.")
    specification = json.loads((ROOT / "Documentation/Evidence/windows-payload-manifest.json").read_text())
    manifest = specification["files"]
    if len(manifest) != 163:
        raise RuntimeError("Unexpected player inventory.")
    work = Path(tempfile.mkdtemp(prefix="NebulaRelease-", dir=os.environ["RUNNER_TEMP"]))
    (work / "existing").mkdir()
    installer = download("NebulaDefender-Setup-x64.exe", work)
    if sha(installer) != specification["installer_sha256"]:
        raise RuntimeError("Published installer differs from verified input.")
    payload = work / "payload"
    subprocess.run([str(installer), "/S", "/QA", "/D=" + str(payload)], check=True,
                   timeout=180, creationflags=subprocess.CREATE_NO_WINDOW)
    for name, expected_sha in manifest.items():
        path = (payload / name).resolve()
        path.relative_to(payload.resolve())
        if not path.is_file() or sha(path) != expected_sha:
            raise RuntimeError("Installed payload differs: " + name)
    print("Verified all 163 installed payload files.", flush=True)

    windows = work / "NebulaDefender-Windows-x64.zip"
    with zipfile.ZipFile(windows, "w", zipfile.ZIP_DEFLATED) as bundle:
        for name in sorted(manifest):
            bundle.write(payload / name, "Windows/" + name)
    verify_windows(windows, manifest)
    windows = upload_or_reuse(windows, lambda path: verify_windows(path, manifest))

    web_root = ROOT / "Distribution/WebGL"
    web_files = {p.relative_to(web_root).as_posix(): sha(p) for p in web_root.rglob("*") if p.is_file()}
    if web_files != specification["web_files"]:
        raise RuntimeError("Compiled web payload differs from the verified local build.")
    prefix = "NebulaDefender-Web-Local/"
    web = work / "NebulaDefender-Web-Local.zip"
    with zipfile.ZipFile(web, "w", zipfile.ZIP_DEFLATED) as bundle:
        for name in sorted(web_files):
            bundle.write(web_root / name, prefix + "Builds/WebGL/" + name)
        bundle.write(windows, prefix + "Builds/NebulaDefender-Windows-x64.zip")
        bundle.write(ROOT / "Tools/serve_web.py", prefix + "Tools/serve_web.py")
        bundle.write(ROOT / "JUGAR_LOCALHOST.cmd", prefix + "JUGAR_LOCALHOST.cmd")
        bundle.write(ROOT / "THIRD_PARTY_NOTICES.md", prefix + "THIRD_PARTY_NOTICES.md")
        bundle.write(ROOT / "Assets/TextMesh Pro/Fonts/LiberationSans - OFL.txt", prefix + "Licenses/LiberationSans-OFL.txt")
        bundle.writestr(prefix + "LEEME.txt", "NEBULA DEFENDER - WEB LOCAL\n"
            "Extrae todo. Con Python 3.10 o posterior, abre JUGAR_LOCALHOST.cmd.\n"
            "Visita http://localhost:8080 y conserva el servidor abierto.\n"
            "Mouse apunta, izquierdo dispara, derecho avanza, Shift escudo, Esc pausa.\n"
            "Despeja los tres sectores y cruza sus portales para volver a casa.\n")

    def verify_web(path):
        with zipfile.ZipFile(path) as bundle:
            if bundle.testzip() is not None:
                raise RuntimeError("Web archive CRC failed.")
            for name, expected_sha in web_files.items():
                if hashlib.sha256(bundle.read(prefix + "Builds/WebGL/" + name)).hexdigest() != expected_sha:
                    raise RuntimeError("Web file differs: " + name)
            inner = bundle.read(prefix + "Builds/NebulaDefender-Windows-x64.zip")
            if hashlib.sha256(inner).hexdigest() != sha(windows):
                raise RuntimeError("Included Windows download does not match the portable release.")
            verify_windows(io.BytesIO(inner), manifest)
    verify_web(web)
    web = upload_or_reuse(web, verify_web)

    current = {a["name"]: a for a in assets() if a["state"] == "uploaded"}
    source = current["NebulaDefender-Unity-Source.zip"]
    if source["digest"] != "sha256:" + specification["source_zip_sha256"]:
        raise RuntimeError("Source archive differs from the published snapshot.")
    filenames = ["NebulaDefender-Setup-x64.exe", windows.name, web.name, source["name"]]
    sums = work / "SHA256SUMS.txt"
    sums.write_text("".join(current[n]["digest"].removeprefix("sha256:") + "  " + n + "\n" for n in filenames), encoding="ascii")
    gh("release", "upload", TAG, str(sums), "--repo", REPO, "--clobber")
    report = {"result": "passed", "method": "Assembled from the verified installer and compiled web payload; Unity was not rebuilt.",
              "payload_files_verified": len(manifest), "web_files_verified": len(web_files),
              "workflow_url": f"https://github.com/{REPO}/actions/runs/{os.environ['GITHUB_RUN_ID']}",
              "assets": [{"name": n, "bytes": current[n]["size"], "sha256": current[n]["digest"].removeprefix("sha256:")} for n in filenames]}
    proof = work / "distribution-verification.json"
    proof.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    gh("release", "upload", TAG, str(proof), "--repo", REPO, "--clobber")
    print(json.dumps(report, indent=2), flush=True)


if __name__ == "__main__":
    main()
