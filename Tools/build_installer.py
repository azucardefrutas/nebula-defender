"""Package the verified Unity player with an already-installed NSIS compiler."""
from pathlib import Path
import hashlib
import json
import os
import subprocess

ROOT = Path(__file__).resolve().parents[1]
PAYLOAD = ROOT / "Builds/Windows"
OUTPUT = ROOT / "Builds/NebulaDefender-Setup-x64.exe"
WORK = ROOT / "BuildWork/Installer"


def main():
    if not (PAYLOAD / "NebulaDefender.exe").is_file():
        raise RuntimeError("Build the Unity Windows player first.")
    compilers = list((Path(os.environ["LOCALAPPDATA"]) / "electron-builder/Cache").glob("nsis*/**/makensis.exe"))
    compiler = next((p for p in compilers if (p.parent / "Include/MUI2.nsh").exists()), None)
    if compiler is None:
        raise RuntimeError("NSIS compiler not found. No software was installed.")
    files = sorted(p for p in PAYLOAD.rglob("*") if p.is_file())
    for p in files:
        p.resolve().relative_to(PAYLOAD.resolve())
    # Uninstallation removes only packaged files, preserving foreign files and saved games.
    deletes = "\n".join('  Delete "$INSTDIR\\' + str(p.relative_to(PAYLOAD)).replace("/", "\\") + '"' for p in files)
    directories = sorted((p for p in PAYLOAD.rglob("*") if p.is_dir()), key=lambda p: len(p.parts), reverse=True)
    removes = "\n".join('  RMDir "$INSTDIR\\' + str(p.relative_to(PAYLOAD)).replace("/", "\\") + '"' for p in directories)
    source = r'''
Unicode true
!include "MUI2.nsh"
!include "FileFunc.nsh"
Name "Nebula Defender"
OutFile "@OUTPUT@"
InstallDir "$LOCALAPPDATA\Programs\NebulaDefender"
RequestExecutionLevel user
SetCompressor /SOLID lzma
VIProductVersion "1.0.0.0"
VIAddVersionKey "ProductName" "Nebula Defender: Galactic Assault"
VIAddVersionKey "FileDescription" "Instalador de Nebula Defender"
VIAddVersionKey "FileVersion" "1.0.0"
VIAddVersionKey "LegalCopyright" "UPB Arcade"
Var QA_MODE
!define MUI_ABORTWARNING
!define MUI_WELCOMEPAGE_TITLE "Tu ruta de regreso comienza aquí"
!define MUI_WELCOMEPAGE_TEXT "Abre tres rutas de salto, derrota a Némesis y vuelve a casa.$\r$\n$\r$\nMouse: apunta. Clic izquierdo: dispara. Clic derecho: avanza.$\r$\n$\r$\nEl juego incluye todos sus archivos y funciona sin Internet."
!define MUI_FINISHPAGE_RUN "$INSTDIR\NebulaDefender.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Jugar ahora"
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_LANGUAGE "Spanish"
Function .onInit
  StrCpy $QA_MODE "0"
  @@{GetParameters} $R0
  ClearErrors
  @@{GetOptions} $R0 "/QA" $R1
  IfErrors +2 0
  StrCpy $QA_MODE "1"
FunctionEnd
Section "Juego"
  SetShellVarContext current
  SetOutPath "$INSTDIR"
  File /r "@PAYLOAD@\*.*"
  WriteINIStr "$INSTDIR\install-state.ini" "Install" "QA" $QA_MODE
  WriteUninstaller "$INSTDIR\Desinstalar.exe"
  StrCmp $QA_MODE "1" RegistrationDone
  CreateDirectory "$SMPROGRAMS\Nebula Defender"
  CreateShortcut "$SMPROGRAMS\Nebula Defender\Nebula Defender.lnk" "$INSTDIR\NebulaDefender.exe"
  CreateShortcut "$DESKTOP\Nebula Defender.lnk" "$INSTDIR\NebulaDefender.exe"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\NebulaDefender" "DisplayName" "Nebula Defender: Galactic Assault"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\NebulaDefender" "DisplayVersion" "1.0.0"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\NebulaDefender" "Publisher" "UPB Arcade"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\NebulaDefender" "InstallLocation" "$INSTDIR"
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\NebulaDefender" "UninstallString" '$\"$INSTDIR\Desinstalar.exe$\"'
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\NebulaDefender" "NoModify" 1
  WriteRegDWORD HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\NebulaDefender" "NoRepair" 1
  RegistrationDone:
SectionEnd
Section "Uninstall"
  SetShellVarContext current
  ReadINIStr $QA_MODE "$INSTDIR\install-state.ini" "Install" "QA"
  StrCmp $QA_MODE "1" SkipRegistrationRemoval
  ReadRegStr $R0 HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\NebulaDefender" "InstallLocation"
  StrCmp $R0 "$INSTDIR" 0 SkipRegistrationRemoval
  Delete "$DESKTOP\Nebula Defender.lnk"
  Delete "$SMPROGRAMS\Nebula Defender\Nebula Defender.lnk"
  RMDir "$SMPROGRAMS\Nebula Defender"
  DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\NebulaDefender"
  SkipRegistrationRemoval:
@DELETES@
@REMOVES@
  Delete "$INSTDIR\install-state.ini"
  Delete "$INSTDIR\Desinstalar.exe"
  RMDir "$INSTDIR"
SectionEnd
'''
    source = source.replace("@@", "$").replace("@OUTPUT@", str(OUTPUT)).replace("@PAYLOAD@", str(PAYLOAD)).replace("@DELETES@", deletes).replace("@REMOVES@", removes)
    WORK.mkdir(parents=True, exist_ok=True)
    script = WORK / "NebulaDefender.nsi"
    script.write_text(source, encoding="utf-8-sig")
    result = subprocess.run([str(compiler), "/V3", str(script)], cwd=ROOT, capture_output=True, text=True, errors="replace")
    (ROOT / "Documentation/Evidence/installer-build.log").write_text(result.stdout + result.stderr, encoding="utf-8")
    if result.returncode or not OUTPUT.is_file():
        raise RuntimeError("Installer compilation failed. Inspect installer-build.log.")
    report = {"file": OUTPUT.name, "bytes": OUTPUT.stat().st_size, "sha256": hashlib.sha256(OUTPUT.read_bytes()).hexdigest(), "payload_files": len(files), "compiler": str(compiler), "result": "compiled"}
    (ROOT / "Documentation/Evidence/installer-build.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
