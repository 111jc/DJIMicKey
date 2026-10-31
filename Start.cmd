@echo off
cd /d "%~dp0"
set "DJI_APP=%~dp0dist\DJIMicKey.exe"
if exist "%~dp0dist\DJIMicKey-v1.0.2.exe" set "DJI_APP=%~dp0dist\DJIMicKey-v1.0.2.exe"
if not exist "%DJI_APP%" (
    echo Run Build.cmd or Release.cmd first.
    pause
    exit /b 1
)
start "" "%DJI_APP%"
