"""Verify the complete Unity Windows ZIP without extracting or executing it."""
from pathlib import Path
import argparse
import hashlib
import json
import zipfile

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--zip", type=Path, default=ROOT / "Builds/NebulaDefender-Windows-x64.zip")
    args = parser.parse_args()
    package = args.zip
    with zipfile.ZipFile(package) as archive:
        bad_entry = archive.testzip()
        if bad_entry:
            raise RuntimeError("ZIP integrity failure: " + bad_entry)
        names = archive.namelist()
        required = ["Windows/NebulaDefender.exe", "Windows/UnityPlayer.dll",
                    "Windows/THIRD_PARTY_NOTICES.md", "Windows/Licenses/LiberationSans-OFL.txt"]
        missing = [name for name in required if name not in names]
        if not any(name in names for name in ["Windows/NebulaDefender_Data/data.unity3d", "Windows/NebulaDefender_Data/globalgamemanagers"]):
            missing.append("Unity scene data (packed or unpacked)")
        if missing or not any(name.startswith("Windows/MonoBleedingEdge/") for name in names):
            raise RuntimeError("Incomplete Unity player: " + ", ".join(missing or ["Mono runtime"]))
    result = {
        "zip": package.name,
        "bytes": package.stat().st_size,
        "entries": len(names),
        "integrity": "passed",
        "sha256": hashlib.sha256(package.read_bytes()).hexdigest(),
        "required_player_files": "passed",
    }
    (ROOT / "Documentation/Evidence/package-inspection.json").write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(result, indent=2))


if __name__ == "__main__":
    main()
