@echo off
cd /d "%~dp0"
if exist "%~dp0dist\DJIMicKey.exe" "%~dp0dist\DJIMicKey.exe" --quit
