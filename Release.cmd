@echo off
setlocal
pushd "%~dp0"
set "DJI_VERSION=1.0.0"
set "DJI_OUTPUT=dist\DJIMicKey-v%DJI_VERSION%.exe"
call Build.cmd "%DJI_OUTPUT%"
if errorlevel 1 goto failed
"%WINDIR%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -NonInteractive -Command "$ErrorActionPreference='Stop'; $version='%DJI_VERSION%'; $stage='.build\release-v'+$version; New-Item -ItemType Directory -Force -Path $stage | Out-Null; Copy-Item -LiteralPath ('dist\DJIMicKey-v'+$version+'.exe') -Destination (Join-Path $stage 'DJIMicKey.exe') -Force; Copy-Item -LiteralPath 'docs\WINDOWS.md' -Destination (Join-Path $stage 'README.txt') -Force; Copy-Item -LiteralPath 'LICENSE' -Destination (Join-Path $stage 'LICENSE') -Force; Copy-Item -LiteralPath 'docs\WIN11_TROUBLESHOOTING.md' -Destination (Join-Path $stage 'WIN11-TROUBLESHOOTING.txt') -Force; Copy-Item -LiteralPath 'docs\POWER_BUTTON_FINDINGS.md' -Destination (Join-Path $stage 'POWER-BUTTON-FINDINGS.txt') -Force; Compress-Archive -Path (Join-Path $stage '*') -DestinationPath ('dist\DJIMicKey-v'+$version+'-portable.zip') -Force"
if errorlevel 1 goto failed
echo Executable: %DJI_OUTPUT%
echo Portable archive: dist\DJIMicKey-v%DJI_VERSION%-portable.zip
popd
exit /b 0
:failed
echo Packaging failed. Close DJI Mic Key before rebuilding.
popd
exit /b 1
