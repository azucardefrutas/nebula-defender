@echo off
setlocal
set "NEBULA_EDITOR=C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe"
if defined UNITY_EDITOR set "NEBULA_EDITOR=%UNITY_EDITOR%"
if not exist "%NEBULA_EDITOR%" (
  echo No se encontro Unity 6000.3.15f1. Abre esta carpeta desde Unity Hub.
  pause
  exit /b 1
)
start "" "%NEBULA_EDITOR%" -projectPath "%~dp0." -executeMethod NebulaDefender.Editor.EditorStartup.OpenCore --burst-disable-compilation
endlocal
