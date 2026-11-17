@echo off
chcp 65001 >nul
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File ".\dotnet.ps1" run --project tests/AcademicRecords.Tests --configuration Release
pause
