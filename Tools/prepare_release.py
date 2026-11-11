"""Prepare versioned GitHub assets from a clean committed project and verified builds."""
from pathlib import Path
import hashlib
import json
import shutil
import subprocess
import zipfile

ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Builds/Release-v1.0.0"


def sha256(path):
    checksum = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            checksum.update(chunk)
    return checksum.hexdigest()


def main():
    if subprocess.check_output(["git", "status", "--porcelain"], cwd=ROOT, text=True).strip():
        raise RuntimeError("Commit the intended source and documentation before preparing a release.")
    commit = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip()
    OUTPUT.mkdir(parents=True, exist_ok=True)
    for name, evidence in [("NebulaDefender-Setup-x64.exe", "installer-build.json"),
                           ("NebulaDefender-Windows-x64.zip", "package-inspection.json")]:
        original = ROOT / "Builds" / name
        report = json.loads((ROOT / "Documentation/Evidence" / evidence).read_text(encoding="utf-8"))
        if sha256(original) != report["sha256"]:
            raise RuntimeError("Verified build changed: " + name)
        shutil.copy2(original, OUTPUT / name)
    if (OUTPUT / "NebulaDefender-Setup-x64.exe").read_bytes()[:2] != b"MZ":
        raise RuntimeError("Installer is not a Windows executable.")

    web = ROOT / "Builds/WebGL"
    if not (web / "index.html").is_file() or not list((web / "Build").glob("*.wasm.gz")):
        raise RuntimeError("Missing actual Web build.")
    prefix = "NebulaDefender-Web-Local/"
    with zipfile.ZipFile(OUTPUT / "NebulaDefender-Web-Local.zip", "w", zipfile.ZIP_DEFLATED) as archive:
        for path in sorted(web.rglob("*")):
            if path.is_file():
                archive.write(path, prefix + "Builds/WebGL/" + path.relative_to(web).as_posix())
        archive.write(ROOT / "Tools/serve_web.py", prefix + "Tools/serve_web.py")
        archive.write(ROOT / "JUGAR_LOCALHOST.cmd", prefix + "JUGAR_LOCALHOST.cmd")
        archive.write(ROOT / "Builds/NebulaDefender-Windows-x64.zip", prefix + "Builds/NebulaDefender-Windows-x64.zip")
        archive.writestr(prefix + "LEEME.txt", "NEBULA DEFENDER 1.0 - WEB LOCAL\n\n"
            "Extrae toda esta carpeta. Con Python 3.10 o posterior instalado, abre JUGAR_LOCALHOST.cmd.\n"
            "Visita http://localhost:8080 y conserva el servidor abierto mientras juegas.\n"
            "No necesitas Unity ni conexion a Internet para jugar con los archivos ya descargados.\n"
            "El boton de descarga Windows entrega el ZIP portable incluido en Builds.\n"
            "Mouse: apunta. Izquierdo: dispara. Derecho: avanza. Rueda/Shift: escudo. Esc: pausa.\n"
            "Despeja cada sector y entra en el centro del portal azul para volver a casa.\n")
    subprocess.run(["git", "archive", "--format=zip", "--prefix=NebulaDefender-Unity/",
                    "--output=" + str(OUTPUT / "NebulaDefender-Unity-Source.zip"), commit], cwd=ROOT, check=True)

    source_required = ["Assets/Nebula/Generated/Scenes/Campaign.unity", "Assets/Nebula/Generated/Scenes/CoreSandbox.unity",
                       "Packages/manifest.json", "ProjectSettings/ProjectVersion.txt", "Documentation/TEST_REPORT.md"]
    archives = {}
    for path in sorted(OUTPUT.glob("*.zip")):
        with zipfile.ZipFile(path) as archive:
            if archive.testzip() is not None:
                raise RuntimeError("ZIP CRC failure: " + path.name)
            names = archive.namelist()
            if path.name.endswith("Unity-Source.zip"):
                if any("NebulaDefender-Unity/" + p not in names for p in source_required):
                    raise RuntimeError("Unity source is incomplete.")
                if sum(n.endswith(".blend") for n in names) != 7 or sum(n.endswith(".fbx") for n in names) != 7:
                    raise RuntimeError("Editable model inventory differs from the release.")
            archives[path.name] = {"entries": len(names), "crc": "passed"}

    filenames = ["NebulaDefender-Setup-x64.exe", "NebulaDefender-Windows-x64.zip",
                 "NebulaDefender-Web-Local.zip", "NebulaDefender-Unity-Source.zip"]
    assets = [{"name": name, "bytes": (OUTPUT/name).stat().st_size, "sha256": sha256(OUTPUT/name)} for name in filenames]
    (OUTPUT / "SHA256SUMS.txt").write_text("".join(a["sha256"] + "  " + a["name"] + "\n" for a in assets), encoding="ascii")
    report = {"result": "passed", "tag": "v1.0.0", "source_commit": commit, "assets": assets, "archives": archives}
    (ROOT / "Documentation/Evidence/release-package-inspection.json").write_text(json.dumps(report, indent=2)+"\n", encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
