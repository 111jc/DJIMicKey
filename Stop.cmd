@echo off
cd /d "%~dp0"
if exist "%~dp0dist\DJIMicKey-v1.0.2.exe" (
    "%~dp0dist\DJIMicKey-v1.0.2.exe" --quit
) else if exist "%~dp0dist\DJIMicKey.exe" (
    "%~dp0dist\DJIMicKey.exe" --quit
)
