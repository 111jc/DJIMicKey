@echo off
cd /d "%~dp0"
if not exist "%~dp0dist\DJIMicKey.exe" (
    echo Run Build.cmd first to create dist\DJIMicKey.exe.
    pause
    exit /b 1
)
start "" "%~dp0dist\DJIMicKey.exe"
