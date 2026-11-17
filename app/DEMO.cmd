@echo off
chcp 65001 >nul
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File ".\dotnet.ps1" run --project src/AcademicRecords.Console --configuration Release -- --demo --data data/demo.json
pause
