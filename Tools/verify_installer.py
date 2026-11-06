"""Exercise the actual NSIS package without changing the user's installation."""
from pathlib import Path
import hashlib
import json
import subprocess
import tempfile
import winreg

ROOT = Path(__file__).resolve().parents[1]
PAYLOAD = ROOT / "Builds/Windows"
INSTALLER = ROOT / "Builds/NebulaDefender-Setup-x64.exe"
REGISTRATION = r"Software\Microsoft\Windows\CurrentVersion\Uninstall\NebulaDefender"


def digest(path):
    checksum = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            checksum.update(chunk)
    return checksum.hexdigest()


def registration():
    try:
        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, REGISTRATION) as key:
            result = {}
            for index in range(winreg.QueryInfoKey(key)[1]):
                name, value, kind = winreg.EnumValue(key, index)
                result[name] = [value, kind]
            return result
    except FileNotFoundError:
        return None


def main():
    if not INSTALLER.is_file() or not (PAYLOAD / "NebulaDefender.exe").is_file():
        raise RuntimeError("Build the Windows player and installer first.")
    expected = {p.relative_to(PAYLOAD): digest(p) for p in PAYLOAD.rglob("*") if p.is_file()}
    before = registration()
    target = Path(tempfile.mkdtemp(prefix="NebulaInstallerQA-")).resolve()
    # The uninstaller may remove only this explicitly created temporary installation.
    target.relative_to(Path(tempfile.gettempdir()).resolve())
    if not target.name.startswith("NebulaInstallerQA-"):
        raise RuntimeError("Unexpected test destination.")
    report = {"target": str(target), "installer": INSTALLER.name,
              "installer_sha256": digest(INSTALLER), "result": "failed"}
    try:
        subprocess.run([str(INSTALLER), "/S", "/QA", "/D=" + str(target)],
                       check=True, timeout=120, creationflags=subprocess.CREATE_NO_WINDOW)
        for relative, checksum in expected.items():
            installed = target / relative
            if not installed.is_file() or digest(installed) != checksum:
                raise RuntimeError("Installed payload differs: " + str(relative))
        if registration() != before:
            raise RuntimeError("QA installation changed the user's registration.")
        sentinel = target / "foreign-file-preservation-check.txt"
        sentinel.write_text("Preserve this unrelated file.\n", encoding="utf-8")
        uninstaller = target / "Desinstalar.exe"
        if not uninstaller.is_file():
            raise RuntimeError("Missing uninstaller.")
        subprocess.run([str(uninstaller), "/S", "_?=" + str(target)],
                       check=True, timeout=120, creationflags=subprocess.CREATE_NO_WINDOW)
        remaining_payload = [str(p) for p in expected if (target / p).exists()]
        if remaining_payload:
            raise RuntimeError("Packaged files survived uninstall: " + repr(remaining_payload[:5]))
        if not sentinel.is_file() or registration() != before:
            raise RuntimeError("Uninstall did not preserve foreign files or user registration.")
        report.update(result="passed", payload_files_checked=len(expected), hashes_match=True,
                      foreign_file_preserved=True, user_registration_untouched=True)
    except Exception as error:
        report["error"] = str(error)
        raise
    finally:
        (ROOT / "Documentation/Evidence/installer-qa.json").write_text(
            json.dumps(report, indent=2) + "\n", encoding="utf-8")
        print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
