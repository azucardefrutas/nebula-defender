@echo off
cd /d "%~dp0"
python Tools\serve_web.py --open --port 8080
if errorlevel 1 pause
